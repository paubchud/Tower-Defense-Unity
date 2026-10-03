using System;
using System.IO;
using TowerDefense.Networking;
using UnityEngine;

namespace TowerDefense.Editor
{
    public static class EosBuildConfiguration
    {
        public static string Destination(string directory, string platform)
        {
            if (platform != VersionedBuild.Windows && platform != VersionedBuild.MacOS) throw new ArgumentException("Unknown platform.");
            return Path.Combine(directory, platform == VersionedBuild.Windows ? "TowerDefense_Data/StreamingAssets/TowerDefense/eos-settings.json"
                : "TowerDefense.app/Contents/Resources/Data/StreamingAssets/TowerDefense/eos-settings.json");
        }
        public static void CopyToBuild(string directory, string platform)
        {
            // Copy AFTER Unity build, outside Assets. No temporary credential-bearing Unity asset/meta.
            var config = EosSettings.Load();
            if (!config.IsConfigured) throw new InvalidOperationException("EOS local game-client configuration is missing/invalid. Previous working build was not promoted.");
            string target = Destination(directory, platform);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.WriteAllText(target, JsonUtility.ToJson(config, true));
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.towerdefense.eos.sdk/package.json");
            if (package == null) throw new InvalidOperationException("Pinned EOS SDK package is missing.");
            File.Copy(Path.Combine(package.resolvedPath, "EOS-NATIVE-LICENSE.txt"), Path.Combine(directory, "EOS-NATIVE-LICENSE.txt"));
            File.Copy(Path.Combine(package.resolvedPath, "LICENSE.md"), Path.Combine(directory, "EOS-SDK-NOTICES.md"));
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            File.Copy(Path.Combine(repository, "EOS_SETUP.md"), Path.Combine(directory, "EOS_SETUP.md"));
            Debug.Log("TD_EOS_BUILD_CONFIG_PASS limited-game-client local-config-not-in-Git");
        }
    }
}
