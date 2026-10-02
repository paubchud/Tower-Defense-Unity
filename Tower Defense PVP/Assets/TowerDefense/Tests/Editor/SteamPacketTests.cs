using System;
using NUnit.Framework;
using TowerDefense.Networking;

namespace TowerDefense.Tests
{
    public sealed class SteamPacketTests
    {
        [Test]
        public void PacketRoundTripRespectsPayloadOffset()
        {
            var packet = SteamPacket.Encode(new ArraySegment<byte>(new byte[] { 0, 1, 2, 3, 4 }, 2, 2), false, 0);
            bool hasSequence = false; uint lastSequence = 0;
            Assert.That(SteamPacket.TryDecode(packet, ref hasSequence, ref lastSequence, out var decoded), Is.True);
            Assert.That(decoded, Is.EqualTo(new byte[] { 2, 3 }));
            Assert.That(hasSequence, Is.False);
        }

        [Test]
        public void SequencedPacketsRejectOlderOrRepeatedDeliveryAndAllowWraparound()
        {
            bool hasSequence = false; uint lastSequence = 0;
            var payload = new ArraySegment<byte>(new byte[] { 1 });
            bool Decode(uint sequence) => SteamPacket.TryDecode(SteamPacket.Encode(payload, true, sequence), ref hasSequence, ref lastSequence, out _);
            Assert.That(Decode(uint.MaxValue - 1), Is.True);
            Assert.That(Decode(uint.MaxValue - 1), Is.False);
            Assert.That(Decode(uint.MaxValue - 2), Is.False);
            Assert.That(Decode(uint.MaxValue), Is.True);
            Assert.That(Decode(0), Is.True);
            Assert.That(Decode(uint.MaxValue), Is.False);
        }

        [Test]
        public void MalformedPacketsAndOversizedPayloadsAreRejected()
        {
            bool hasSequence = false; uint lastSequence = 0;
            Assert.That(SteamPacket.TryDecode(null, ref hasSequence, ref lastSequence, out _), Is.False);
            Assert.That(SteamPacket.TryDecode(new byte[4], ref hasSequence, ref lastSequence, out _), Is.False);
            Assert.That(SteamPacket.TryDecode(new byte[] { 2, 0, 0, 0, 0 }, ref hasSequence, ref lastSequence, out _), Is.False);
            Assert.Throws<ArgumentException>(() => SteamPacket.Encode(default, false, 0));
            Assert.Throws<ArgumentException>(() => SteamPacket.Encode(new ArraySegment<byte>(new byte[SteamPacket.MaximumPayload + 1]), false, 0));
        }
    }
}
