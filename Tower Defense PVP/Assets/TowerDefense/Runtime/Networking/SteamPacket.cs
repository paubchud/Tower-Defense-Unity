using System;

namespace TowerDefense.Networking
{
    // Steam reliability is native; explicitly enforce the NGO unreliable-sequenced contract.
    public static class SteamPacket
    {
        public const int HeaderSize = 5;
        public const int MaximumPayload = Steamworks.Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend - HeaderSize;

        public static byte[] Encode(ArraySegment<byte> payload, bool sequenced, uint sequence)
        {
            if (payload.Array == null || payload.Count > MaximumPayload) throw new ArgumentException("Invalid Steam packet payload.");
            var packet = new byte[payload.Count + HeaderSize];
            packet[0] = sequenced ? (byte)1 : (byte)0;
            for (int i = 0; i < 4; i++) packet[i + 1] = (byte)(sequence >> (i * 8));
            Buffer.BlockCopy(payload.Array, payload.Offset, packet, HeaderSize, payload.Count);
            return packet;
        }

        public static bool TryDecode(byte[] packet, ref bool hasSequence, ref uint lastSequence, out ArraySegment<byte> payload)
        {
            payload = default;
            if (packet == null || packet.Length < HeaderSize || packet.Length > MaximumPayload + HeaderSize || packet[0] > 1) return false;
            if (packet[0] == 1)
            {
                uint sequence = 0;
                for (int i = 0; i < 4; i++) sequence |= (uint)packet[i + 1] << (i * 8);
                if (hasSequence && unchecked((int)(sequence - lastSequence)) <= 0) return false;
                lastSequence = sequence;
                hasSequence = true;
            }
            payload = new ArraySegment<byte>(packet, HeaderSize, packet.Length - HeaderSize);
            return true;
        }
    }
}
