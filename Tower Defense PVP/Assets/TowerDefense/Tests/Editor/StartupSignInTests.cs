using System;
using System.IO;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;

namespace TowerDefense.Tests
{
    public sealed class StartupSignInTests
    {
        private string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "td-sign-in-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            // Only this fixture's uniquely created directory, never the shared temp root.
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [TestCase("steam")]
        [TestCase("steam-test")]
        public void SteamAutomaticallyUnlocksMenuAndCannotBeReplaced(string provider)
        {
            var flow = new StartupSignIn();
            var account = new AccountIdentity(provider, "1234");
            Assert.That(flow.FinishSteamCheck(flow.BeginSteamCheck(), account, "Player", null), Is.True);
            Assert.That(flow.State, Is.EqualTo(StartupSignInState.SignedIn));
            Assert.That(flow.Account, Is.EqualTo(account));
            Assert.That(flow.DisplayName, Is.EqualTo("Player"));
            Assert.That(flow.ContinueAsGuest(() => throw new Exception("Must not load guest profile")), Is.False);
            Assert.Throws<InvalidOperationException>(() => flow.BeginSteamCheck());
        }

        [Test]
        public void FailureNeedsGuestConsentAndCanRetrySteam()
        {
            var flow = new StartupSignIn();
            int attempt = flow.BeginSteamCheck();
            Assert.That(flow.ContinueAsGuest(() => throw new Exception("Too early")), Is.False);
            flow.FinishSteamCheck(attempt, default, null, "Steam unavailable");
            Assert.That(flow.State, Is.EqualTo(StartupSignInState.GuestChoice));
            Assert.That(flow.Account.IsValid, Is.False);
            Assert.That(flow.Status, Is.EqualTo("Steam unavailable"));
            flow.FinishSteamCheck(flow.BeginSteamCheck(), new AccountIdentity("steam", "7"), "Player", null);
            Assert.That(flow.State, Is.EqualTo(StartupSignInState.SignedIn));
        }

        [Test]
        public void LateSteamResultCannotReplaceNewerCheckOrGuest()
        {
            var flow = new StartupSignIn();
            int first = flow.BeginSteamCheck();
            int second = flow.BeginSteamCheck();
            Assert.That(flow.FinishSteamCheck(first, new AccountIdentity("steam", "7"), "Player", null), Is.False);
            flow.FinishSteamCheck(second, default, null, null);
            var guest = new GuestProfileStore(directory).LoadOrCreate();
            Assert.That(flow.ContinueAsGuest(() => guest), Is.True);
            Assert.That(flow.FinishSteamCheck(second, new AccountIdentity("steam", "7"), "Player", null), Is.False);
            Assert.That(flow.Account, Is.EqualTo(guest));
        }

        [Test]
        public void StorageFailureOrWrongProviderDoesNotPretendToSignIn()
        {
            var flow = new StartupSignIn();
            flow.FinishSteamCheck(flow.BeginSteamCheck(), default, null, null);
            Assert.That(flow.ContinueAsGuest(() => throw new IOException("private-path")), Is.False);
            Assert.That(flow.State, Is.EqualTo(StartupSignInState.GuestChoice));
            Assert.That(flow.Account.IsValid, Is.False);
            Assert.That(flow.Status, Does.Not.Contain("private-path"));
            Assert.That(flow.ContinueAsGuest(() => new AccountIdentity("steam", "7")), Is.False);
        }

        [TestCase(null, "Steam Player")]
        [TestCase(" ", "Steam Player")]
        [TestCase("name\ncontrol", "Steam Player")]
        [TestCase(" Player ", "Player")]
        public void DisplayNameIsNotProfileIdentity(string name, string expected)
        {
            var flow = new StartupSignIn();
            var account = new AccountIdentity("steam", "7");
            flow.FinishSteamCheck(flow.BeginSteamCheck(), account, name, null);
            Assert.That(flow.DisplayName, Is.EqualTo(expected));
            Assert.That(flow.Account, Is.EqualTo(account));
        }

        [Test]
        public void NonSteamProbeCannotBypassGuestConsent()
        {
            var flow = new StartupSignIn();
            flow.FinishSteamCheck(flow.BeginSteamCheck(), new AccountIdentity("guest", "7"), "Guest", null);
            Assert.That(flow.State, Is.EqualTo(StartupSignInState.GuestChoice));
            Assert.That(flow.Account.IsValid, Is.False);
        }

        [Test]
        public void GuestPersistsAcrossInstancesWithBackupAndNoTemporaryFiles()
        {
            var account = new GuestProfileStore(directory).LoadOrCreate();
            Assert.That(account.Provider, Is.EqualTo("guest"));
            Assert.That(Guid.TryParseExact(account.Subject, "N", out _), Is.True);
            Assert.That(new GuestProfileStore(directory).LoadOrCreate(), Is.EqualTo(account));
            Assert.That(File.Exists(Path.Combine(directory, "guest-profile.json.bak")), Is.True);
            Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
            Assert.That(account, Is.Not.EqualTo(new AccountIdentity("steam", account.Subject)));
        }

        [Test]
        public void CorruptPrimaryUsesBackupWithoutOverwritingEither()
        {
            var account = new GuestProfileStore(directory).LoadOrCreate();
            string primary = Path.Combine(directory, "guest-profile.json");
            string backup = File.ReadAllText(primary + ".bak");
            File.WriteAllText(primary, "broken");
            Assert.That(new GuestProfileStore(directory).LoadOrCreate(), Is.EqualTo(account));
            Assert.That(File.ReadAllText(primary), Is.EqualTo("broken"));
            Assert.That(File.ReadAllText(primary + ".bak"), Is.EqualTo(backup));
        }

        [Test]
        public void MissingPrimaryRecoversBackup()
        {
            var account = new GuestProfileStore(directory).LoadOrCreate();
            File.Delete(Path.Combine(directory, "guest-profile.json"));
            Assert.That(new GuestProfileStore(directory).LoadOrCreate(), Is.EqualTo(account));
        }

        [TestCase("broken")]
        [TestCase("{}")]
        [TestCase("{\"schemaVersion\":1,\"id\":\"../profile\"}")]
        [TestCase("{\"schemaVersion\":1,\"id\":\"00000000000000000000000000000000\"}")]
        public void UnrecoverableProfileIsNeverReplaced(string contents)
        {
            string primary = Path.Combine(directory, "guest-profile.json");
            File.WriteAllText(primary, contents);
            Assert.Throws<InvalidDataException>(() => new GuestProfileStore(directory).LoadOrCreate());
            Assert.That(File.ReadAllText(primary), Is.EqualTo(contents));
        }

        [Test]
        public void UnknownVersionIsNotDowngradedToBackup()
        {
            new GuestProfileStore(directory).LoadOrCreate();
            string primary = Path.Combine(directory, "guest-profile.json");
            File.WriteAllText(primary, "{\"schemaVersion\":2,\"id\":\"future-format\"}");
            Assert.Throws<NotSupportedException>(() => new GuestProfileStore(directory).LoadOrCreate());
            Assert.That(File.ReadAllText(primary), Does.Contain("future-format"));
        }

        [Test]
        public void OversizedProfileIsRejected()
        {
            File.WriteAllText(Path.Combine(directory, "guest-profile.json"), new string('x', 4097));
            Assert.Throws<InvalidDataException>(() => new GuestProfileStore(directory).LoadOrCreate());
        }

        [Test]
        public void GuestPreparationDoesNotUseSteamOrClaimWorkingInternet()
        {
            var provider = new UnconfiguredGuestProvider();
            Assert.That(provider.SupportsFriendInvites, Is.False);
            Assert.That(provider.ShowInviteOverlay(), Is.False);
            Assert.That(provider.TryNormalizeCode("1234", out _), Is.False);
            Assert.That(provider.PrepareAsync(true, "").IsFaulted, Is.True);
            Assert.That(provider.DescribeFailure(null), Does.Contain("not connected"));
        }
    }
}
