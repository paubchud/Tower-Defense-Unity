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
    }
}
