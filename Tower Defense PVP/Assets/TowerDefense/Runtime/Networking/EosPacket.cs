using System;
using System.Collections.Generic;

namespace TowerDefense.Networking
{
    // EOS P2P frames are limited to 1170 bytes. NGO's reliable fragmented messages
    // need bounded reassembly rather than silently truncating an initial spawn/handshake.
    public static class EosPacket
    {
        public const int FrameSize = 1170, HeaderSize = 14, ChunkSize = FrameSize - HeaderSize, MaximumPayload = 65536;
        public static IEnumerable<byte[]> Encode(ArraySegment<byte> payload, byte channel, uint message)
        {
            if (payload.Array == null || payload.Count < 1 || payload.Count > MaximumPayload || channel < 1 || channel > 4
                || (channel <= 2 && payload.Count > ChunkSize)) throw new ArgumentException("Invalid EOS packet.");
            int count = (payload.Count + ChunkSize - 1) / ChunkSize;
            for (int index = 0; index < count; index++)
            {
                int length = Math.Min(ChunkSize, payload.Count - index * ChunkSize);
                var frame = new byte[HeaderSize + length];
                frame[0] = 1; frame[1] = channel; Write(frame, 2, message, 4);
                Write(frame, 6, (uint)index, 2); Write(frame, 8, (uint)count, 2); Write(frame, 10, (uint)payload.Count, 4);
                Buffer.BlockCopy(payload.Array, payload.Offset + index * ChunkSize, frame, HeaderSize, length);
                yield return frame;
            }
        }
        private static void Write(byte[] data, int offset, uint value, int count)
        { for (int i = 0; i < count; i++) data[offset + i] = (byte)(value >> (8 * i)); }
        private static uint Read(byte[] data, int offset, int count)
        { uint value = 0; for (int i = 0; i < count; i++) value |= (uint)data[offset + i] << (8 * i); return value; }

        public sealed class Receiver
        {
            private sealed class Assembly
            {
                public byte[] Data;
                public bool[] Seen;
                public int Received;
                public double Started;
                public byte Channel;
            }
            private readonly Dictionary<ulong, Assembly> assemblies = new Dictionary<ulong, Assembly>();
            private readonly uint[] lastSequence = new uint[5];
            private readonly bool[] hasSequence = new bool[5];
            public int PendingCount => assemblies.Count;
            public bool HasExpired(double now)
            {
                foreach (var item in assemblies.Values) if (now - item.Started > 10) return true;
                return false;
            }
            public bool TryReceive(byte[] frame, int length, byte channel, double now, out ArraySegment<byte> payload)
            {
                payload = default;
                if (frame == null || length < HeaderSize + 1 || length > FrameSize || length > frame.Length
                    || frame[0] != 1 || channel < 1 || channel > 4 || frame[1] != channel) return false;
                uint id = Read(frame, 2, 4); int index = (int)Read(frame, 6, 2), count = (int)Read(frame, 8, 2), total = (int)Read(frame, 10, 4);
                if (total < 1 || total > MaximumPayload || count != (total + ChunkSize - 1) / ChunkSize || index >= count
                    || length - HeaderSize != Math.Min(ChunkSize, total - index * ChunkSize) || (channel <= 2 && count != 1)) return false;
                if (channel == 2 && hasSequence[channel] && unchecked((int)(id - lastSequence[channel])) <= 0) return false;
                if (count == 1)
                {
                    var data = new byte[total]; Buffer.BlockCopy(frame, HeaderSize, data, 0, total);
                    if (channel == 2) { lastSequence[channel] = id; hasSequence[channel] = true; }
                    payload = new ArraySegment<byte>(data); return true;
                }
                ulong key = ((ulong)channel << 32) | id;
                if (!assemblies.TryGetValue(key, out var assembly))
                {
                    if (assemblies.Count >= 8) return false;
                    assembly = new Assembly { Data = new byte[total], Seen = new bool[count], Started = now, Channel = channel };
                    assemblies.Add(key, assembly);
                }
                if (assembly.Data.Length != total || assembly.Seen.Length != count || assembly.Seen[index]) return false;
                Buffer.BlockCopy(frame, HeaderSize, assembly.Data, index * ChunkSize, length - HeaderSize);
                assembly.Seen[index] = true; assembly.Received++;
                if (assembly.Received != count) return false;
                assemblies.Remove(key); payload = new ArraySegment<byte>(assembly.Data); return true;
            }
        }
    }
}
