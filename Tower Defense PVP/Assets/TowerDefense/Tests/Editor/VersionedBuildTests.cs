using System;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using TowerDefense.Editor;
using UnityEngine;

namespace TowerDefense.Tests
{
    public sealed class VersionedBuildTests
    {
        private string repository;

        [SetUp]
        public void SetUp()
        {
            repository = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "TowerDefense-BuildTest-" + Guid.NewGuid().ToString("N"))).FullName;
            File.WriteAllText(Path.Combine(repository, "TowerDefense.exe.lnk"), "old launcher");
            File.WriteAllText(Path.Combine(repository, "MAC_TESTING.md"), "fixture Mac instructions");
        }

        [TearDown]
        public void TearDown()
        {
            // This is only the unique, owned fixture directory created in SetUp.
            if (Directory.Exists(repository)) Directory.Delete(repository, true);
        }

        [TestCase("0.1")]
        [TestCase("0.1.1")]
        [TestCase("12.25")]
        public void NumericVersionsAreAccepted(string version) => Assert.DoesNotThrow(() => VersionedBuild.ValidateVersion(version));

        [TestCase(null)]
        [TestCase("")]
        [TestCase("../0.1")]
        [TestCase("0.1/other")]
        [TestCase("0.1\n")]
        [TestCase("0.1.2.3")]
        [TestCase("01.1")]
        public void InvalidVersionsCannotBecomeBuildPaths(string version) => Assert.Throws<ArgumentException>(() => VersionedBuild.ValidateVersion(version));

        [Test]
        public void RebuildingDoesNotOverwritePreviousFoldersOrArchives()
        {
            string first = VersionedBuild.NextDirectory(repository, "0.1");
            Assert.That(Path.GetFileName(first), Is.EqualTo("TowerDefense-0.1-Windows"));
            Directory.CreateDirectory(first);
            File.WriteAllText(first + "-build2.zip", "existing artifact");
            Assert.That(VersionedBuild.NextDirectory(repository, "0.1"), Is.EqualTo(first + "-build3"));
            Assert.That(Path.GetFileName(VersionedBuild.NextDirectory(repository, "0.2")), Is.EqualTo("TowerDefense-0.2-Windows"));
        }

        [Test]
        public void MissingOrExternalBuildCannotChangeTheCurrentLauncher()
        {
            string shortcut = Path.Combine(repository, "TowerDefense.exe.lnk");
            File.WriteAllText(shortcut, "old launcher");
            string directory = VersionedBuild.NextDirectory(repository, "0.1");
            Directory.CreateDirectory(directory);
            Assert.Throws<FileNotFoundException>(() => VersionedBuild.Publish(repository, directory, "0.1"));
            Assert.Throws<ArgumentException>(() => VersionedBuild.Publish(repository, repository, "0.1"));
            Assert.That(File.ReadAllText(shortcut), Is.EqualTo("old launcher"));
            Assert.That(File.Exists(Path.Combine(repository, "Builds/latest-build.json")), Is.False);
        }

        [Test]
        public void ArchiveContainsPlayableFilesButNotUnityDebugBackups()
        {
            string directory = VersionedBuild.NextDirectory(repository, "0.1");
            Directory.CreateDirectory(Path.Combine(directory, "TowerDefense_Data"));
            Directory.CreateDirectory(Path.Combine(directory, "TowerDefense_BackUpThisFolder_ButDontShipItWithYourGame"));
            File.WriteAllText(Path.Combine(directory, "TowerDefense.exe"), "fixture exe");
            File.WriteAllText(Path.Combine(directory, "UnityPlayer.dll"), "fixture runtime");
            File.WriteAllText(Path.Combine(directory, "steam_appid.txt"), "480");
            File.WriteAllText(Path.Combine(directory, "TowerDefense_Data/globalgamemanagers"), "fixture data");
            File.WriteAllText(Path.Combine(directory, "TowerDefense_BackUpThisFolder_ButDontShipItWithYourGame/debug"), "do not ship");
            VersionedBuild.CreateArchive(directory, directory + ".zip");
            using (var zip = ZipFile.OpenRead(directory + ".zip"))
            {
                Assert.That(zip.GetEntry("TowerDefense.exe"), Is.Not.Null);
                Assert.That(zip.GetEntry("UnityPlayer.dll"), Is.Not.Null);
                Assert.That(zip.GetEntry("TowerDefense_Data/globalgamemanagers"), Is.Not.Null);
                Assert.That(zip.Entries.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public void SuccessfulPublishUpdatesTheSameLauncherAndRelativeManifest()
        {
            string directory = VersionedBuild.NextDirectory(repository, "0.1");
            Directory.CreateDirectory(directory);
            string executable = Path.Combine(directory, "TowerDefense.exe");
            File.WriteAllText(executable, "fixture exe");
            string shortcut = Path.Combine(repository, "TowerDefense.exe.lnk");
            File.WriteAllText(shortcut, "old launcher");
            VersionedBuild.Publish(repository, directory, "0.1", (temporary, target, version) =>
            {
                Assert.That(target, Is.EqualTo(executable));
                Assert.That(version, Is.EqualTo("0.1"));
                Assert.That(File.Exists(directory + ".zip"), Is.True);
                File.WriteAllText(temporary, target);
            });
            Assert.That(File.ReadAllText(shortcut), Is.EqualTo(executable));
            var info = JsonUtility.FromJson<VersionedBuild.BuildInfo>(File.ReadAllText(Path.Combine(repository, "Builds/latest-build.json")));
            Assert.That(info.version, Is.EqualTo("0.1"));
            Assert.That(info.executable, Is.EqualTo("TowerDefense-0.1-Windows/TowerDefense.exe"));
            Assert.That(info.archive, Is.EqualTo("TowerDefense-0.1-Windows.zip"));
            Assert.That(info.privateSteamPlaytestAvailable, Is.True);
            Assert.That(File.ReadAllText(Path.Combine(directory, "Start-Private-Steam-Test.cmd")), Does.Contain("-td-steam-playtest"));
        }

        [Test]
        public void ShortcutFailureLeavesPreviousPublicationIntact()
        {
            string directory = VersionedBuild.NextDirectory(repository, "0.1");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "TowerDefense.exe"), "fixture exe");
            string shortcut = Path.Combine(repository, "TowerDefense.exe.lnk");
            string manifest = Path.Combine(repository, "Builds/latest-build.json");
            File.WriteAllText(shortcut, "old launcher");
            File.WriteAllText(manifest, "old manifest");
            Assert.Throws<IOException>(() => VersionedBuild.Publish(repository, directory, "0.1", (_, __, ___) => throw new IOException("fixture failure")));
            Assert.That(File.ReadAllText(shortcut), Is.EqualTo("old launcher"));
            Assert.That(File.ReadAllText(manifest), Is.EqualTo("old manifest"));
            Assert.That(Directory.GetFiles(Path.Combine(repository, "Builds"), ".*"), Is.Empty);
        }

        [Test]
        public void SuccessfulPublicationKeepsOnlyTheCurrentBuildAndPreservesOtherFiles()
        {
            string previous = CreateFixtureBuild("0.1");
            VersionedBuild.Publish(repository, previous, "0.1", WriteFixtureShortcut);
            string buildRoot = Path.Combine(repository, "Builds");
            string validation = Directory.CreateDirectory(Path.Combine(buildRoot, "Validation")).FullName;
            File.WriteAllText(Path.Combine(validation, "test.log"), "keep evidence");
            File.WriteAllText(Path.Combine(buildRoot, "personal.zip"), "not a generated build");
            Directory.CreateDirectory(Path.Combine(buildRoot, "InternetPrototype"));
            File.WriteAllText(Path.Combine(buildRoot, "TowerDefense-Internet-20261001.zip"), "legacy generated ZIP");
            string current = CreateFixtureBuild("0.2");
            VersionedBuild.Publish(repository, current, "0.2", WriteFixtureShortcut);
            Assert.That(Directory.Exists(previous), Is.False);
            Assert.That(File.Exists(previous + ".zip"), Is.False);
            Assert.That(Directory.Exists(Path.Combine(buildRoot, "InternetPrototype")), Is.False);
            Assert.That(File.Exists(Path.Combine(buildRoot, "TowerDefense-Internet-20261001.zip")), Is.False);
            Assert.That(File.Exists(Path.Combine(current, "TowerDefense.exe")), Is.True);
            Assert.That(File.Exists(current + ".zip"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(validation, "test.log")), Is.EqualTo("keep evidence"));
            Assert.That(File.Exists(Path.Combine(buildRoot, "personal.zip")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(repository, "TowerDefense.exe.lnk")), Is.EqualTo(Path.Combine(current, "TowerDefense.exe")));
        }

        [Test]
        public void FailedNewPublicationDoesNotRemoveTheWorkingBuild()
        {
            string previous = CreateFixtureBuild("0.1");
            VersionedBuild.Publish(repository, previous, "0.1", WriteFixtureShortcut);
            string current = CreateFixtureBuild("0.2");
            Assert.Throws<IOException>(() => VersionedBuild.Publish(repository, current, "0.2",
                (_, __, ___) => throw new IOException("fixture failure")));
            Assert.That(File.Exists(Path.Combine(previous, "TowerDefense.exe")), Is.True);
            Assert.That(File.Exists(previous + ".zip"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(repository, "TowerDefense.exe.lnk")), Is.EqualTo(Path.Combine(previous, "TowerDefense.exe")));
        }

        [Test]
        public void UnpublishedOrExternalCurrentDirectoryCannotTriggerCleanup()
        {
            string previous = CreateFixtureBuild("0.1");
            VersionedBuild.Publish(repository, previous, "0.1", WriteFixtureShortcut);
            string unpublished = CreateFixtureBuild("0.2");
            Assert.Throws<InvalidOperationException>(() => VersionedBuild.RemoveOlderBuilds(repository, unpublished));
            Assert.Throws<ArgumentException>(() => VersionedBuild.RemoveOlderBuilds(repository, repository));
            Assert.That(File.Exists(previous + ".zip"), Is.True);
            Assert.That(Directory.Exists(previous), Is.True);
        }

        [Test]
        public void RemovedShortcutStaysRemovedAndCleanupStillKeepsTheNewBuild()
        {
            string previous = CreateFixtureBuild("0.1");
            VersionedBuild.Publish(repository, previous, "0.1", WriteFixtureShortcut);
            File.Delete(Path.Combine(repository, "TowerDefense.exe.lnk"));
            string current = CreateFixtureBuild("0.2");
            VersionedBuild.Publish(repository, current, "0.2",
                (_, __, ___) => Assert.Fail("A removed shortcut must not be recreated."));
            Assert.That(File.Exists(Path.Combine(repository, "TowerDefense.exe.lnk")), Is.False);
            Assert.That(Directory.Exists(previous), Is.False);
            Assert.That(File.Exists(current + ".zip"), Is.True);
        }

        private string CreateFixtureBuild(string version)
        {
            string directory = VersionedBuild.NextDirectory(repository, version);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "TowerDefense.exe"), "fixture exe");
            return directory;
        }

        private static void WriteFixtureShortcut(string temporary, string executable, string version)
            => File.WriteAllText(temporary, executable);

        [Test]
        public void MacAndWindowsPublicationKeepOneLatestBuildPerPlatform()
        {
            string windows = CreateFixtureBuild("0.1.3");
            VersionedBuild.Publish(repository, windows, "0.1.3", WriteFixtureShortcut);
            string mac = CreateMacFixture("0.1.3");
            VersionedBuild.Publish(repository, mac, "0.1.3", (_, __, ___) => Assert.Fail("Mac must not update Windows shortcut."), platform: VersionedBuild.MacOS);
            Assert.That(File.Exists(windows + ".zip"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(repository, "TowerDefense.exe.lnk")), Is.EqualTo(Path.Combine(windows, "TowerDefense.exe")));
            var info = JsonUtility.FromJson<VersionedBuild.BuildInfo>(File.ReadAllText(Path.Combine(repository, "Builds/latest-build-macOS.json")));
            Assert.That(info.platform, Is.EqualTo("macOS"));
            Assert.That(info.architecture, Is.EqualTo("x86_64+arm64"));
            Assert.That(info.executable, Does.EndWith("TowerDefense.app/Contents/MacOS/Tower Defense PVP"));
            Assert.That(File.Exists(Path.Combine(mac, "MAC_TESTING.md")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(mac, "Start-Private-Steam-Test.command")), Does.Contain("-td-steam-playtest"));
            string nextMac = CreateMacFixture("0.1.4");
            VersionedBuild.Publish(repository, nextMac, "0.1.4", platform: VersionedBuild.MacOS);
            Assert.That(Directory.Exists(mac), Is.False);
            Assert.That(File.Exists(mac + ".zip"), Is.False);
            Assert.That(File.Exists(windows + ".zip"), Is.True);
            string nextWindows = CreateFixtureBuild("0.1.4");
            VersionedBuild.Publish(repository, nextWindows, "0.1.4", WriteFixtureShortcut);
            Assert.That(File.Exists(windows + ".zip"), Is.False);
            Assert.That(File.Exists(nextMac + ".zip"), Is.True);
            Assert.That(File.Exists(nextWindows + ".zip"), Is.True);
        }

        [Test]
        public void InvalidMacPackageRetainsPreviousWorkingDownloadsAndManifest()
        {
            string previous = CreateMacFixture("0.1.3");
            VersionedBuild.Publish(repository, previous, "0.1.3", platform: VersionedBuild.MacOS);
            string manifest = Path.Combine(repository, "Builds/latest-build-macOS.json");
            string oldManifest = File.ReadAllText(manifest);
            string broken = CreateMacFixture("0.1.4");
            File.WriteAllText(Path.Combine(broken, "TowerDefense.app/Contents/MacOS/Tower Defense PVP"), "not a universal executable");
            Assert.Throws<IOException>(() => VersionedBuild.Publish(repository, broken, "0.1.4", platform: VersionedBuild.MacOS));
            Assert.That(File.ReadAllText(manifest), Is.EqualTo(oldManifest));
            Assert.That(Directory.Exists(previous), Is.True);
            Assert.That(File.Exists(previous + ".zip"), Is.True);
            Assert.Throws<IOException>(() => VersionedBuild.Publish(repository, previous, "0.1.4", platform: VersionedBuild.MacOS));
            Assert.Throws<ArgumentException>(() => VersionedBuild.NextDirectory(repository, "0.1.3", "../other"));
        }

        [Test]
        public void MacZipPreservesExecutableModesAndUnixCreatorMetadata()
        {
            string mac = CreateMacFixture("0.1.3");
            VersionedBuild.Publish(repository, mac, "0.1.3", platform: VersionedBuild.MacOS);
            using (var zip = ZipFile.OpenRead(mac + ".zip"))
            {
                foreach (string name in new[] { "TowerDefense.app/Contents/MacOS/Tower Defense PVP", "Start-Private-Steam-Test.command", "TowerDefense.app/Contents/Frameworks/UnityPlayer.dylib" })
                    Assert.That((uint)zip.GetEntry(name).ExternalAttributes >> 16, Is.EqualTo(0x81EDu));
                Assert.That((uint)zip.GetEntry("MAC_TESTING.md").ExternalAttributes >> 16, Is.EqualTo(0x81A4u));
            }
            using (var reader = new BinaryReader(File.OpenRead(mac + ".zip")))
            {
                reader.BaseStream.Position = reader.BaseStream.Length - 6;
                uint central = reader.ReadUInt32();
                reader.BaseStream.Position = central;
                Assert.That(reader.ReadUInt32(), Is.EqualTo(0x02014B50u));
                reader.ReadByte();
                Assert.That(reader.ReadByte(), Is.EqualTo(3));
            }
        }

        [Test]
        public void MacUniversalCheckRejectsSingleArchitecture()
        {
            string mac = CreateMacFixture("0.1.3");
            string executable = Path.Combine(mac, MacBuildArtifacts.ExecutablePath(mac));
            using (var stream = File.OpenWrite(executable))
            {
                stream.Position = 28; // second architecture type
                stream.Write(new byte[] { 1, 0, 0, 7 }, 0, 4);
            }
            Assert.Throws<IOException>(() => MacBuildArtifacts.RequireUniversal(executable));
        }

        private string CreateMacFixture(string version)
        {
            string directory = VersionedBuild.NextDirectory(repository, version, VersionedBuild.MacOS);
            string contents = Path.Combine(directory, "TowerDefense.app/Contents");
            Directory.CreateDirectory(Path.Combine(contents, "MacOS"));
            Directory.CreateDirectory(Path.Combine(contents, "Frameworks"));
            Directory.CreateDirectory(Path.Combine(contents, "Plugins/steam_api.bundle/Contents/MacOS"));
            File.WriteAllText(Path.Combine(contents, "Info.plist"), "<plist><dict><key>CFBundleExecutable</key><string>Tower Defense PVP</string><key>CFBundleShortVersionString</key><string>" + version + "</string></dict></plist>");
            foreach (string relative in new[] { "MacOS/Tower Defense PVP", "Frameworks/UnityPlayer.dylib", "Plugins/steam_api.bundle/Contents/MacOS/libsteam_api.dylib" })
            {
                // Minimal architecture table fixture, not a runnable binary.
                var bytes = new byte[48];
                bytes[0] = 0xCA; bytes[1] = 0xFE; bytes[2] = 0xBA; bytes[3] = 0xBE; bytes[7] = 2;
                bytes[8] = 1; bytes[11] = 7; bytes[28] = 1; bytes[31] = 12;
                File.WriteAllBytes(Path.Combine(contents, relative), bytes);
            }
            return directory;
        }
    }
}
