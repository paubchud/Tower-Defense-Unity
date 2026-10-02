using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;
using Unity.Netcode.Transports.UTP;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TowerDefense.Tests
{
    public sealed class SteamCodeTests
    {
        [TestCase(" 109775241000000000 ", "109775241000000000")]
        [TestCase("109775241000000001", "109775241000000001")]
        public void PastedCodesNormalizeWithoutChangingTheirMeaning(string input, string expected)
        {
            Assert.That(SteamLobbyCode.TryNormalize(input, out var code), Is.True);
            Assert.That(code, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("0")]
        [TestCase("76561198000000000")]
        [TestCase("109775241 000000000")]
        [TestCase("184467440737095516160")]
        [TestCase("https://example.com")]
        public void MalformedCodesAreRejectedBeforeAnyServiceCall(string input)
        {
            Assert.That(SteamLobbyCode.TryNormalize(input, out var code), Is.False);
            Assert.That(code, Is.Empty);
        }
    }

    public sealed class OnlineConnectionTests
    {
        private PrototypeSession session;
        private FakeOnlineProvider rooms;

        private sealed class FakeOnlineProvider : IOnlineProvider
        {
            public int Calls;
            public bool GuestCodes;
            public string DisplayName => GuestCodes ? "Guest test" : "Steam";
            public string PendingInvite => string.Empty;
            public bool SupportsFriendInvites => false;
            public event Action HostLost;
            public readonly TaskCompletionSource<OnlineConnection> Pending = new TaskCompletionSource<OnlineConnection>();
            public Task<OnlineConnection> PrepareAsync(bool host, string code) { Calls++; return Pending.Task; }
            public bool TryNormalizeCode(string input, out string code)
            {
                if (!GuestCodes) return SteamLobbyCode.TryNormalize(input, out code);
                code = input?.Trim(); return code == "guest-room";
            }
            public void Leave() { }
            public bool ShowInviteOverlay() => false;
            public string DescribeFailure(Exception error) => SteamLobbyService.DescribeFailure(error);
            public void LoseHost() => HostLost?.Invoke();
        }

        [UnitySetUp]
        public IEnumerator OpenMenu()
        {
            EditorSceneManager.OpenScene("Assets/TowerDefense/Scenes/MainMenu.unity");
            yield return new EnterPlayMode();
            yield return null;
            session = PrototypeSession.Instance;
            Assert.That(session, Is.Not.Null);
            rooms = new FakeOnlineProvider();
            session.SetOnlineProvider(rooms);
        }

        [UnityTearDown]
        public IEnumerator CloseMenu() { yield return new ExitPlayMode(); }

        [UnityTest]
        public IEnumerator CancelAndDuplicateClicksCannotStartALateConnection()
        {
            Task first = session.ConnectOnlineAsync(true, "warrior");
            Task duplicate = session.ConnectOnlineAsync(true, "wizard");
            Assert.That(session.Connecting, Is.True);
            Assert.That(rooms.Calls, Is.EqualTo(1));
            Assert.That(duplicate.IsCompleted, Is.True);
            session.Leave();
            rooms.Pending.SetResult(default);
            double deadline = Time.realtimeSinceStartupAsDouble + 5;
            while ((!first.IsCompleted || !session.CanConnect) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(first.IsCompletedSuccessfully, Is.True);
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Manager.IsListening, Is.False);
            Assert.That(session.JoinCode, Is.Empty);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
        }

        [UnityTest]
        public IEnumerator FailedRoomAllowsRetryWithoutLeakingServiceDetails()
        {
            Task failed = session.ConnectOnlineAsync(false, "wizard", "109775241000000000");
            rooms.Pending.SetException(new InvalidOperationException("private-service-response"));
            while (!failed.IsCompleted) yield return null;
            Assert.That(session.Status, Does.Contain("Could not connect through Steam"));
            Assert.That(session.Status, Does.Not.Contain("private-service-response"));
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Manager.IsListening, Is.False);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
            rooms = new FakeOnlineProvider();
            session.SetOnlineProvider(rooms);
            Task retry = session.ConnectOnlineAsync(true, "warrior");
            Assert.That(rooms.Calls, Is.EqualTo(1));
            session.Leave();
            rooms.Pending.SetException(new TimeoutException());
            while (!retry.IsCompleted || !session.CanConnect) yield return null;
            Assert.That(session.Status, Does.Not.Contain("timed out"), "A canceled attempt must not replace the menu's status.");
        }

        [UnityTest]
        public IEnumerator InvalidCodeDoesNotAllocateOrLeaveTheMenu()
        {
            Task attempt = session.ConnectOnlineAsync(false, "wizard", "bad-url");
            yield return null;
            Assert.That(attempt.IsCompletedSuccessfully, Is.True);
            Assert.That(rooms.Calls, Is.Zero);
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Status, Does.Contain("room code"));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
        }

        [UnityTest]
        public IEnumerator ProviderCannotChangeWhileConnecting()
        {
            var attempt = session.ConnectOnlineAsync(true, "warrior");
            Assert.Throws<InvalidOperationException>(() => session.SetOnlineProvider(new FakeOnlineProvider()));
            Assert.That(session.OnlineProvider, Is.SameAs(rooms));
            session.Leave(); rooms.Pending.SetResult(default);
            while (!attempt.IsCompleted || !session.CanConnect) yield return null;
        }

        [UnityTest]
        public IEnumerator NonSteamProviderSuppliesItsOwnIdentityCodeAndTransport()
        {
            rooms.GuestCodes = true;
            var localTransport = session.GetComponent<UnityTransport>();
            localTransport.SetConnectionData("127.0.0.1", 7788, "127.0.0.1");
            rooms.Pending.SetResult(new OnlineConnection("guest-room", localTransport, new AccountIdentity("guest", "fixture-local-profile")));
            var attempt = session.ConnectOnlineAsync(true, "wizard");
            double deadline = Time.realtimeSinceStartupAsDouble + 8;
            while ((!attempt.IsCompleted || session.LocalHero == null) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(attempt.IsCompletedSuccessfully, Is.True);
            Assert.That(session.Manager.IsHost, Is.True);
            Assert.That(session.JoinCode, Is.EqualTo("guest-room"));
            Assert.That(session.LocalAccount.ProfileKey, Is.EqualTo("guest:fixture-local-profile"));
            Assert.That(session.Manager.NetworkConfig.NetworkTransport, Is.SameAs(localTransport));
            Assert.That(session.SteamService.InitializedAppId, Is.Zero, "An alternate provider must not require Steam.");
            session.Leave();
            while (!session.CanConnect && Time.realtimeSinceStartupAsDouble < deadline + 5) yield return null;
            Assert.That(session.LocalAccount.IsValid, Is.False);
            Assert.That(session.CanConnect, Is.True);
        }
    }
}
