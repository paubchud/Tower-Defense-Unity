using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using UnityEngine;
using TowerDefense.Networking;

namespace TowerDefense.Editor
{
    public static class VersionedBuild
    {
        [Serializable]
        public sealed class BuildInfo
        {
            public string version;
            public string platform;
            public string architecture;
            public string executable;
            public string archive;
            public string builtAtUtc;
            public string onlineProvider;
            public uint steamAppId;
            public bool privateSteamPlaytestAvailable;
        }

        public static void ValidateVersion(string version)
        {
            if (version == null || !Regex.IsMatch(version, @"\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){1,2}\z"))
                throw new ArgumentException("Set Player Settings > Version to a numeric version such as 0.1 or 0.1.1.");
        }

        public const string Windows = "Windows";
        public const string MacOS = "macOS";

        private static void ValidatePlatform(string platform)
        {
            if (platform != Windows && platform != MacOS) throw new ArgumentException("Unknown build platform.");
        }

        public static string ManifestName(string platform) => platform == Windows ? "latest-build.json" : "latest-build-macOS.json";

        public static string NextDirectory(string repository, string version, string platform = Windows)
        {
            ValidateVersion(version);
            ValidatePlatform(platform);
            string stem = Path.Combine(Path.GetFullPath(repository), "Builds", "TowerDefense-" + version + "-" + platform);
            string candidate = stem;
            for (int build = 2; Directory.Exists(candidate) || File.Exists(candidate) || File.Exists(candidate + ".zip"); build++)
                candidate = stem + "-build" + build;
            return candidate;
        }

        public static void CreateArchive(string directory, string archive, string platform = Windows)
        {
            ValidatePlatform(platform);
            string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            using (var output = new FileStream(archive, FileMode.CreateNew))
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create))
            {
                foreach (string file in EnumerateSafeFiles(root))
                {
                    string relative = file.Substring(root.Length).Replace('\\', '/');
                    // Unity explicitly marks this directory as not for distribution.
                    if (Array.Exists(relative.Split('/'), part => part.EndsWith("_BackUpThisFolder_ButDontShipItWithYourGame", StringComparison.Ordinal))) continue;
                    if (string.Equals(Path.GetFileName(file), "steam_appid.txt", StringComparison.OrdinalIgnoreCase)) continue;
                    var entry = zip.CreateEntry(relative, System.IO.Compression.CompressionLevel.Optimal);
                    if (platform == MacOS)
                        entry.ExternalAttributes = unchecked((int)((MacBuildArtifacts.IsExecutable(file) ? 0x81EDu : 0x81A4u) << 16));
                    using (var input = File.OpenRead(file))
                    using (var destination = entry.Open()) input.CopyTo(destination);
                }
            }
            if (platform == MacOS) MacBuildArtifacts.MarkZipAsUnix(archive);
        }

        private static IEnumerable<string> EnumerateSafeFiles(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                string path = pending.Pop();
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Packaging will not follow links: " + path);
                if ((attributes & FileAttributes.Directory) != 0)
                    foreach (string child in Directory.EnumerateFileSystemEntries(path)) pending.Push(child);
                else yield return path;
            }
        }

        public static void Publish(string repository, string directory, string version,
            Action<string, string, string> writeShortcut = null, bool developmentBuild = true, string platform = Windows)
        {
            ValidateVersion(version);
            ValidatePlatform(platform);
            string buildRoot = Path.Combine(Path.GetFullPath(repository), "Builds");
            directory = Path.GetFullPath(directory);
            // Publishing only touches an immediate build directory, never an arbitrary external path.
            if (!string.Equals(Path.GetDirectoryName(directory), buildRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The build must be directly inside this repository's Builds folder.");
            if (!IsBuildDirectoryName(Path.GetFileName(directory), platform)) throw new ArgumentException("Use a versioned build directory for this platform.");
            string relativeExecutable = platform == Windows ? "TowerDefense.exe" : MacBuildArtifacts.ExecutablePath(directory);
            string executable = Path.Combine(directory, relativeExecutable);
            if (!File.Exists(executable)) throw new FileNotFoundException("Cannot publish a build without its executable.", executable);
            if (platform == MacOS) MacBuildArtifacts.Validate(directory, version);
            string archive = directory + ".zip";
            if (File.Exists(archive)) throw new IOException("The build archive already exists. Create a new build instead of replacing it.");
            var info = new BuildInfo
            {
                version = version,
                platform = platform,
                architecture = platform == Windows ? "x86_64" : "x86_64+arm64",
                executable = Path.GetFileName(directory) + "/" + relativeExecutable.Replace('\\', '/'),
                archive = Path.GetFileName(archive),
                builtAtUtc = DateTime.UtcNow.ToString("o"),
                onlineProvider = "Steam",
                steamAppId = Resources.Load<SteamSettings>("SteamSettings")?.AppId ?? 0,
                privateSteamPlaytestAvailable = developmentBuild
            };
            if (developmentBuild)
            {
                if (platform == Windows)
                    File.WriteAllText(Path.Combine(directory, "Start-Private-Steam-Test.cmd"),
                        "@echo off\r\ncd /d \"%~dp0\"\r\nstart \"\" \"%~dp0TowerDefense.exe\" -td-steam-playtest\r\n");
                else
                    File.WriteAllText(Path.Combine(directory, "Start-Private-Steam-Test.command"),
                        "#!/bin/sh\nset -eu\ncd -- \"$(dirname -- \"$0\")\"\nopen -n \"./TowerDefense.app\" --args -td-steam-playtest\n");
                File.WriteAllText(Path.Combine(directory, "PRIVATE-STEAM-TEST.txt"),
                    "EXPERIMENTAL STEAM DEVELOPMENT PLAYTEST - Tower Defense v" + version + "\r\n\r\n"
                    + "Sign into Steam. Both friends extract this whole ZIP. Windows: Start-Private-Steam-Test.cmd.\r\n"
                    + "Mac: read MAC_TESTING.md, then open TowerDefense.app or Start-Private-Steam-Test.command.\r\n"
                    + "Choose a class and click ENABLE PRIVATE STEAM TEST (480) if not already enabled.\r\n"
                    + "Host Steam, copy the numeric room code, and share it privately. Friend uses Join Steam. Both Ready.\r\n"
                    + "Use separate Steam accounts/devices and matching builds. Keep the host game open.\r\n"
                    + "Steam may show Spacewar: this uses Valve's shared example App ID 480, not our production identity.\r\n"
                    + "Both players should already have this game running; an invite can launch Spacewar if it is closed.\r\n"
                    + "Public experimental download only; not guest play, production Steam distribution, or a completed combat game.\r\n"
                    + "Real two-device replication/reachability still needs testing; report host/client logs if it fails.\r\n");
            }
            if (platform == MacOS) File.Copy(Path.Combine(repository, "MAC_TESTING.md"), Path.Combine(directory, "MAC_TESTING.md"));
            string json = JsonUtility.ToJson(info, true);
            File.WriteAllText(Path.Combine(directory, "build-info.json"), json);

            string id = Guid.NewGuid().ToString("N");
            string temporaryArchive = Path.Combine(buildRoot, ".archive-" + id + ".tmp");
            string temporaryManifest = Path.Combine(buildRoot, ".manifest-" + id + ".tmp");
            string temporaryShortcut = Path.Combine(buildRoot, ".launcher-" + id + ".lnk");
            string manifest = Path.Combine(buildRoot, ManifestName(platform));
            string manifestBackup = Path.Combine(buildRoot, ".previous-manifest-" + id + ".tmp");
            string launcher = Path.Combine(repository, "TowerDefense.exe.lnk");
            try
            {
                CreateArchive(directory, temporaryArchive, platform);
                File.Move(temporaryArchive, archive);
                File.WriteAllText(temporaryManifest, json);
                // Update an existing shortcut, but respect an owner's choice to remove it.
                bool updateLauncher = platform == Windows && File.Exists(launcher);
                if (updateLauncher) (writeShortcut ?? WriteWindowsShortcut)(temporaryShortcut, executable, version);
                bool hadManifest = File.Exists(manifest);
                if (hadManifest) File.Replace(temporaryManifest, manifest, manifestBackup);
                else File.Move(temporaryManifest, manifest);
                try
                {
                    if (updateLauncher && File.Exists(launcher)) ReplaceFile(temporaryShortcut, launcher);
                }
                catch
                {
                    // A failed launcher update must not make the manifest claim publication succeeded.
                    if (hadManifest) File.Replace(manifestBackup, manifest, null);
                    else File.Delete(manifest);
                    throw;
                }
            }
            finally
            {
                foreach (string temporary in new[] { temporaryArchive, temporaryManifest, temporaryShortcut, manifestBackup })
                    if (File.Exists(temporary)) File.Delete(temporary);
            }
            // Prune only after the ZIP, manifest and any existing launcher are promoted successfully.
            // A locked older game must not invalidate the new, working publication.
            try { RemoveOlderBuilds(repository, directory, platform); }
            catch (Exception error) { UnityEngine.Debug.LogWarning("Older build cleanup skipped: " + error.Message); }
            UnityEngine.Debug.Log("TD_PUBLISH_PASS v" + version + " " + archive);
        }

        public static void RemoveOlderBuilds(string repository, string currentDirectory, string platform = Windows)
        {
            ValidatePlatform(platform);
            string buildRoot = Path.Combine(Path.GetFullPath(repository), "Builds");
            currentDirectory = Path.GetFullPath(currentDirectory);
            if (!string.Equals(Path.GetDirectoryName(currentDirectory), buildRoot, StringComparison.OrdinalIgnoreCase)
                || !IsBuildDirectoryName(Path.GetFileName(currentDirectory), platform))
                throw new ArgumentException("The current build must be a named build directly inside Builds.");
            if ((File.GetAttributes(buildRoot) & FileAttributes.ReparsePoint) != 0
                || (File.GetAttributes(currentDirectory) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Do not clean through a linked Builds/current directory.");
            string currentArchive = currentDirectory + ".zip";
            string relativeExecutable = platform == Windows ? "TowerDefense.exe" : MacBuildArtifacts.ExecutablePath(currentDirectory);
            string manifest = Path.Combine(buildRoot, ManifestName(platform));
            var info = File.Exists(manifest) ? JsonUtility.FromJson<BuildInfo>(File.ReadAllText(manifest)) : null;
            if (info == null
                || !string.Equals(info.executable, Path.GetFileName(currentDirectory) + "/" + relativeExecutable.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(info.archive, Path.GetFileName(currentArchive), StringComparison.OrdinalIgnoreCase)
                || !File.Exists(Path.Combine(currentDirectory, relativeExecutable))
                || !File.Exists(currentArchive))
                throw new InvalidOperationException("Only the successfully published current build may remove older builds.");

            foreach (string directory in Directory.GetDirectories(buildRoot))
                if (IsBuildDirectoryName(Path.GetFileName(directory), platform)
                    && !string.Equals(directory, currentDirectory, StringComparison.OrdinalIgnoreCase))
                    RemoveOldArtifact(buildRoot, directory, true);
            foreach (string archive in Directory.GetFiles(buildRoot, "*.zip"))
                if ((IsBuildDirectoryName(Path.GetFileNameWithoutExtension(archive), platform)
                    || (platform == Windows && Regex.IsMatch(Path.GetFileName(archive), @"\ATowerDefense-Internet-[0-9]{8}\.zip\z")))
                    && !string.Equals(archive, currentArchive, StringComparison.OrdinalIgnoreCase))
                    RemoveOldArtifact(buildRoot, archive, false);
        }

        private static bool IsBuildDirectoryName(string name, string platform) => (platform == Windows && (name == "Prototype" || name == "InternetPrototype"))
            || Regex.IsMatch(name, @"\ATowerDefense-(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){1,2}-" + platform + @"(-build[1-9][0-9]*)?\z");

        private static void RemoveOldArtifact(string buildRoot, string path, bool directory)
        {
            path = Path.GetFullPath(path);
            // Resolve and check the exact leaf before any recursive deletion. Never delete Builds itself.
            if (!string.Equals(Path.GetDirectoryName(path), buildRoot, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Cleanup target is outside Builds.");
            try
            {
                var pending = new Stack<string>();
                pending.Push(path);
                while (pending.Count > 0)
                {
                    string entry = pending.Pop();
                    var attributes = File.GetAttributes(entry);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                        throw new IOException("Cleanup will not follow links: " + entry);
                    if ((attributes & FileAttributes.Directory) != 0)
                        foreach (string child in Directory.EnumerateFileSystemEntries(entry)) pending.Push(child);
                }
                if (directory) Directory.Delete(path, true);
                else File.Delete(path);
                UnityEngine.Debug.Log("TD_BUILD_CLEANUP " + path);
            }
            catch (IOException error) { UnityEngine.Debug.LogWarning("Older build retained: " + error.Message); }
            catch (UnauthorizedAccessException error) { UnityEngine.Debug.LogWarning("Older build retained: " + error.Message); }
        }

        private static void ReplaceFile(string source, string destination)
        {
            if (File.Exists(destination)) File.Replace(source, destination, null);
            else File.Move(source, destination);
        }

        private static void WriteWindowsShortcut(string path, string executable, string version)
        {
            // Windows PowerShell supports Shell COM; the editor's Mono runtime is not relied on for COM interop.
            string script = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "../Tools/UpdateBuildShortcut.ps1"));
            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script
                    + "\" -ShortcutPath \"" + path + "\" -ExecutablePath \"" + executable + "\" -Version \"" + version + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true
            }))
            {
                if (process == null) throw new IOException("Could not start the Windows shortcut helper.");
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    throw new IOException("The Windows shortcut helper timed out.");
                }
                if (process.ExitCode != 0) throw new IOException("Windows shortcut creation failed: " + process.StandardError.ReadToEnd());
            }
        }
    }
}
