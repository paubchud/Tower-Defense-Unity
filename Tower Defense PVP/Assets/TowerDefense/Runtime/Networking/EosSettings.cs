using System;
using System.IO;
using UnityEngine;

namespace TowerDefense.Networking
{
    [Serializable]
    public sealed class EosSettings
    {
        public int schemaVersion = 1;
        public string productId, sandboxId, deploymentId, clientId, clientSecret;
        public bool forceRelay = true;
        public const string MissingMessage = "EOS guest play needs local configuration. Enter the limited Peer2Peer client credentials in .local/eos-settings.json, then rebuild. LAN remains available.";
        public bool IsConfigured => schemaVersion == 1 && HexId(productId) && HexId(sandboxId) && HexId(deploymentId)
            && SafeCredential(clientId) && SafeCredential(clientSecret);
        private static bool HexId(string value)
        {
            if (value == null || value.Length != 32) return false;
            foreach (char c in value) if (!Uri.IsHexDigit(c)) return false;
            return true;
        }
        private static bool SafeCredential(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 64 || value != value.Trim()) return false;
            foreach (char c in value) if (char.IsControl(c) || char.IsWhiteSpace(c)) return false;
            return true;
        }
        public static string LocalPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../.local/eos-settings.json"));
        public static EosSettings Load()
        {
            string path = Application.isEditor ? LocalPath : Path.Combine(Application.streamingAssetsPath, "TowerDefense/eos-settings.json");
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 4096 || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    return new EosSettings();
                return JsonUtility.FromJson<EosSettings>(File.ReadAllText(path)) ?? new EosSettings();
            }
            catch { return new EosSettings(); } // No secret, paths or JSON in logs/errors.
        }
    }
}
