using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace TowerDefense.Editor
{
    // Windows cross-builds lose POSIX modes. Package Mach-O binaries and launchers as Unix 0755.
    public static class MacBuildArtifacts
    {
        private static XmlDocument ReadPlist(string directory)
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(Path.Combine(directory, "TowerDefense.app/Contents/Info.plist"));
            return document;
        }

        private static string PlistValue(XmlDocument document, string key)
            => document.SelectSingleNode("/plist/dict/key[text()='" + key + "']/following-sibling::string[1]")?.InnerText;

        public static string ExecutablePath(string directory)
        {
            string name = PlistValue(ReadPlist(directory), "CFBundleExecutable");
            if (string.IsNullOrEmpty(name) || name == "." || name == ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
                throw new IOException("The Mac bundle must declare a safe executable filename.");
            return "TowerDefense.app/Contents/MacOS/" + name;
        }

        public static void Validate(string directory, string version)
        {
            if (PlistValue(ReadPlist(directory), "CFBundleShortVersionString") != version)
                throw new IOException("Mac bundle version does not match the release version.");
            RequireUniversal(Path.Combine(directory, ExecutablePath(directory)));
            foreach (string filename in new[] { "UnityPlayer.dylib", "libsteam_api.dylib" })
            {
                var paths = Directory.GetFiles(Path.Combine(directory, "TowerDefense.app"), filename, SearchOption.AllDirectories);
                if (paths.Length != 1) throw new IOException("Expected exactly one Mac native library: " + filename);
                RequireUniversal(paths[0]);
            }
            var eosPaths = Directory.GetFiles(Path.Combine(directory, "TowerDefense.app"), "libEOSSDK-Mac-Shipping.dylib", SearchOption.AllDirectories);
            foreach (string eosPath in eosPaths) RequireUniversal(eosPath);
        }

        private static uint ReadBigEndian(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (bytes.Length != 4) throw new EndOfStreamException();
            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        public static void RequireUniversal(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                uint magic = ReadBigEndian(reader);
                if (magic != 0xCAFEBABE && magic != 0xCAFEBABF) throw new IOException("Not a universal Mac binary: " + path);
                uint count = ReadBigEndian(reader);
                if (count < 2 || count > 32) throw new IOException("Invalid Mac architecture table.");
                bool intel = false, silicon = false;
                for (int i = 0; i < count; i++)
                {
                    uint cpu = ReadBigEndian(reader);
                    intel |= cpu == 0x01000007;
                    silicon |= cpu == 0x0100000C;
                    reader.BaseStream.Seek(magic == 0xCAFEBABE ? 16 : 28, SeekOrigin.Current);
                }
                if (!intel || !silicon) throw new IOException("Both Intel and Apple Silicon are required: " + path);
            }
        }

        public static bool IsExecutable(string path)
        {
            if (path.EndsWith(".command", StringComparison.Ordinal)) return true;
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                if (reader.BaseStream.Length < 4) return false;
                uint magic = ReadBigEndian(reader);
                return new uint[] { 0xCAFEBABE, 0xCAFEBABF, 0xFEEDFACE, 0xFEEDFACF, 0xCEFAEDFE, 0xCFFAEDFE }.Contains(magic);
            }
        }

        public static void MarkZipAsUnix(string archive)
        {
            // ZipArchive on Windows writes a DOS creator OS, whose extractors may ignore POSIX modes.
            // Change only our newly created ZIP's central-directory creator byte; retain compression/CRC.
            using (var stream = new FileStream(archive, FileMode.Open, FileAccess.ReadWrite))
            using (var reader = new BinaryReader(stream))
            using (var writer = new BinaryWriter(stream))
            {
                long end = stream.Length - 22; // Our ZIP has no archive comment.
                stream.Position = end;
                if (reader.ReadUInt32() != 0x06054B50) throw new IOException("Invalid ZIP end record.");
                stream.Position = end + 10;
                ushort count = reader.ReadUInt16();
                uint size = reader.ReadUInt32(), offset = reader.ReadUInt32();
                if (count == ushort.MaxValue || size == uint.MaxValue || offset == uint.MaxValue)
                    throw new IOException("Mac packaging does not support ZIP64; use a smaller build.");
                long current = offset;
                for (int i = 0; i < count; i++)
                {
                    stream.Position = current;
                    if (reader.ReadUInt32() != 0x02014B50) throw new IOException("Invalid ZIP central directory.");
                    stream.Position = current + 5;
                    writer.Write((byte)3); // Unix
                    stream.Position = current + 28;
                    ushort name = reader.ReadUInt16(), extra = reader.ReadUInt16(), comment = reader.ReadUInt16();
                    current += 46 + name + extra + comment;
                }
                if (current != (long)offset + size) throw new IOException("Invalid ZIP central directory length.");
            }
        }
    }
}
