using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;
using Unity.Services.Relay;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TowerDefense.Tests
{
    public sealed class RelayCodeTests
    {
        [TestCase(" bCdF67 ", "BCDF67")]
        [TestCase("6789BCDFGHJK", "6789BCDFGHJK")]
        public void PastedCodesNormalizeWithoutChangingTheirMeaning(string input, string expected)
        {
            Assert.That(RelayJoinCode.TryNormalize(input, out var code), Is.True);
            Assert.That(code, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("BCDF6")]
        [TestCase("6789BCDFGHJKL")]
        [TestCase("BC DF67")]
        [TestCase("BCDF00")]
        [TestCase("https://example.com")]
        public void MalformedCodesAreRejectedBeforeAnyServiceCall(string input)
        {
            Assert.That(RelayJoinCode.TryNormalize(input, out var code), Is.False);
            Assert.That(code, Is.Empty);
        }
    }

    public sealed class OnlineConnectionTests
    {
        private PrototypeSession session;
        private FakeRelay relay;

        private sealed class FakeRelay : IRelayConnector
        {
            public int Calls;
            public readonly TaskCompletionSource<RelayConnection> Pending = new TaskCompletionSource<RelayConnection>();
            public Task<RelayConnection> HostAsync() { Calls++; return Pending.Task; }
            public Task<RelayConnection> JoinAsync(string code) { Calls++; return Pending.Task; }
        }

        [UnitySetUp]
        public IEnumerator OpenMenu()
        {
            EditorSceneManager.OpenScene("Assets/TowerDefense/Scenes/MainMenu.unity");
            yield return new EnterPlayMode();
            yield return null;
            session = PrototypeSession.Instance;
            Assert.That(session, Is.Not.Null);
            (session.Relay as IDisposable)?.Dispose();
            relay = new FakeRelay();
            session.Relay = relay;
        }

        [UnityTearDown]
        public IEnumerator CloseMenu() { yield return new ExitPlayMode(); }

        [UnityTest]
        public IEnumerator CancelAndDuplicateClicksCannotStartALateConnection()
        {
            Task first = session.ConnectOnlineAsync(true, "warrior");
            Task duplicate = session.ConnectOnlineAsync(true, "wizard");
            Assert.That(session.Connecting, Is.True);
            Assert.That(relay.Calls, Is.EqualTo(1));
            Assert.That(duplicate.IsCompleted, Is.True);
            session.Leave();
            relay.Pending.SetResult(default);
            double deadline = Time.realtimeSinceStartupAsDouble + 5;
            while ((!first.IsCompleted || !session.CanConnect) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(first.IsCompletedSuccessfully, Is.True);
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Manager.IsListening, Is.False);
            Assert.That(session.JoinCode, Is.Empty);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
        }

        [UnityTest]
        public IEnumerator ExpiredRoomAllowsRetryWithoutLeakingServiceDetails()
        {
            Task failed = session.ConnectOnlineAsync(false, "wizard", "bcdf67");
            relay.Pending.SetException(new RelayServiceException(RelayExceptionReason.JoinCodeNotFound, "private-service-response"));
            while (!failed.IsCompleted) yield return null;
            Assert.That(session.Status, Does.Contain("expired"));
            Assert.That(session.Status, Does.Not.Contain("private-service-response"));
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Manager.IsListening, Is.False);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
            relay = new FakeRelay();
            session.Relay = relay;
            Task retry = session.ConnectOnlineAsync(true, "warrior");
            Assert.That(relay.Calls, Is.EqualTo(1));
            session.Leave();
            relay.Pending.SetException(new TimeoutException());
            while (!retry.IsCompleted || !session.CanConnect) yield return null;
            Assert.That(session.Status, Does.Not.Contain("timed out"), "A canceled attempt must not replace the menu's status.");
        }

        [UnityTest]
        public IEnumerator InvalidCodeDoesNotAllocateOrLeaveTheMenu()
        {
            Task attempt = session.ConnectOnlineAsync(false, "wizard", "bad-url");
            yield return null;
            Assert.That(attempt.IsCompletedSuccessfully, Is.True);
            Assert.That(relay.Calls, Is.Zero);
            Assert.That(session.CanConnect, Is.True);
            Assert.That(session.Status, Does.Contain("room code"));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
        }
    }
}
