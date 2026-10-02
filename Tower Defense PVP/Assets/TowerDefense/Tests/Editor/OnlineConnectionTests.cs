using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;
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
        private FakeSteamRooms rooms;

        private sealed class FakeSteamRooms : ISteamRooms
        {
            public int Calls;
            public readonly TaskCompletionSource<SteamRoom> Pending = new TaskCompletionSource<SteamRoom>();
            public Task<SteamRoom> HostAsync() { Calls++; return Pending.Task; }
            public Task<SteamRoom> JoinAsync(string code) { Calls++; return Pending.Task; }
            public void Leave() { }
            public bool IsMember(ulong id) => false;
        }

        [UnitySetUp]
        public IEnumerator OpenMenu()
        {
            EditorSceneManager.OpenScene("Assets/TowerDefense/Scenes/MainMenu.unity");
            yield return new EnterPlayMode();
            yield return null;
            session = PrototypeSession.Instance;
            Assert.That(session, Is.Not.Null);
            rooms = new FakeSteamRooms();
            session.SteamRooms = rooms;
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
            rooms = new FakeSteamRooms();
            session.SteamRooms = rooms;
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
    }
}
