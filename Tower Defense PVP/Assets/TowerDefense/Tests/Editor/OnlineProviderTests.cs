using System;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;

namespace TowerDefense.Tests
{
    public sealed class OnlineProviderTests
    {
        [Test]
        public void SteamGuestAndPrivateProfilesCannotAlias()
        {
            var steam = new AccountIdentity("steam", "123456789");
            var guest = new AccountIdentity("guest", "123456789");
            var test = new AccountIdentity("steam-test", "123456789");
            Assert.That(steam.Equals(guest), Is.False);
            Assert.That(steam.Equals(test), Is.False);
            Assert.That(guest.ProfileKey, Is.Not.EqualTo(steam.ProfileKey));
            Assert.That(steam.Equals(new AccountIdentity("steam", "123456789")), Is.True);
            Assert.That(default(AccountIdentity).IsValid, Is.False);
            Assert.That(default(AccountIdentity).ProfileKey, Is.Empty);
        }

        [TestCase("Steam", "123")]
        [TestCase("guest", "../profile")]
        [TestCase("guest", "other:123")]
        [TestCase("", "123")]
        [TestCase("guest", null)]
        public void ProfileKeysRejectAmbiguousOrUnsafeParts(string provider, string subject)
            => Assert.Throws<ArgumentException>(() => new AccountIdentity(provider, subject));

        [TestCase(0u)]
        [TestCase(480u)]
        public void NormalPlayDoesNotSilentlyUseTheSampleApp(uint appId)
            => Assert.Throws<SteamOnlineException>(() => SteamLaunchPolicy.ResolveAppId(appId, true, false));

        [Test]
        public void ExplicitPrivatePlaytestUses480OnlyInDevelopment()
        {
            Assert.That(SteamLaunchPolicy.ResolveAppId(0, true, true), Is.EqualTo(480));
            Assert.That(SteamLaunchPolicy.ResolveAppId(123, true, true), Is.EqualTo(480));
            Assert.Throws<SteamOnlineException>(() => SteamLaunchPolicy.ResolveAppId(0, false, true));
            Assert.That(SteamLaunchPolicy.ResolveAppId(123, false, false), Is.EqualTo(123));
        }
    }
}
