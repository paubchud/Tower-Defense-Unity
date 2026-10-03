using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TowerDefense.Core
{
    // Only a stable, device-local identity is saved here: no currency, unlocks, EOS
    // credentials or recovery promise. EOS Device ID will be a separate credential.
    public sealed class GuestProfileStore
    {
        [Serializable]
        private sealed class Profile
        {
            public int schemaVersion;
            public string id;
        }

        private readonly string path;
        private readonly string backup;

        public GuestProfileStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("A profile directory is required.");
            path = Path.Combine(Path.GetFullPath(directory), "guest-profile.json");
            backup = path + ".bak";
        }

        public AccountIdentity LoadOrCreate()
        {
            if (File.Exists(path))
            {
                try { return Read(path); }
                catch (InvalidDataException) { if (File.Exists(backup)) return Read(backup); throw; }
            }
            if (File.Exists(backup)) return Read(backup);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var profile = new Profile { schemaVersion = 1, id = Guid.NewGuid().ToString("N") };
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(profile, true));
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush(true);
                }
                try { File.Move(temporary, path); }
                catch (IOException) when (File.Exists(path)) { return Read(path); } // Another process won first creation.
                // An interrupted backup write must never invalidate the committed identity.
                try { File.Copy(path, backup, false); } catch (IOException) { }
                return new AccountIdentity("guest", profile.id);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        private static AccountIdentity Read(string filename)
        {
            var info = new FileInfo(filename);
            if ((info.Attributes & FileAttributes.ReparsePoint) != 0 || info.Length > 4096)
                throw new InvalidDataException("Invalid guest profile file.");
            Profile profile;
            try { profile = JsonUtility.FromJson<Profile>(File.ReadAllText(filename, Encoding.UTF8)); }
            catch (ArgumentException) { throw new InvalidDataException("Unreadable guest profile."); }
            if (profile == null || profile.schemaVersion == 0)
                throw new InvalidDataException("Unreadable guest profile.");
            if (profile.schemaVersion != 1) throw new NotSupportedException("Guest profile version requires migration.");
            if (!Guid.TryParseExact(profile.id, "N", out var id) || id == Guid.Empty)
                throw new InvalidDataException("Unreadable guest profile identity.");
            return new AccountIdentity("guest", id.ToString("N"));
        }
    }
}
