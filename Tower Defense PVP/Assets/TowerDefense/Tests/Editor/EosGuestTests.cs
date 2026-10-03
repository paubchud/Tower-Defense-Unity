using System;
using System.Linq;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Networking;

namespace TowerDefense.Tests
{
    public sealed class EosGuestTests
    {
        [Test]
        public void GuestCodeHasSeparateNamespaceAndBoundedSocket()
        {
            string code = EosRoomCode.Create();
            Assert.That(EosRoomCode.TryNormalize(" " + code.ToLowerInvariant() + " ", out var normalized), Is.True);
            Assert.That(normalized, Is.EqualTo(code));
            Assert.That(EosRoomCode.SocketName(code).Length, Is.LessThanOrEqualTo(32));
            Assert.That(EosRoomCode.SocketName(code), Is.Not.EqualTo(EosRoomCode.SocketName(EosRoomCode.Create())));
            Assert.That(SteamLobbyCode.TryNormalize(code, out _), Is.False);
        }
        [TestCase(null)] [TestCase("")] [TestCase("1234")] [TestCase("TDG../file")]
        [TestCase("TDGXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX")]
        public void InvalidCodeIsRejectedBeforeOnlineCalls(string code) => Assert.That(EosRoomCode.TryNormalize(code, out _), Is.False);

        [Test]
        public void ConfigurationAndErrorsDoNotExposeSecrets()
        {
            Assert.That(new EosSettings().IsConfigured, Is.False);
            var config = new EosSettings { productId = new string('a', 32), sandboxId = new string('b', 32), deploymentId = new string('c', 32), clientId = "gameclient", clientSecret = "fake-test-secret" };
            Assert.That(config.IsConfigured, Is.True);
            config.schemaVersion = 2; Assert.That(config.IsConfigured, Is.False);
            Assert.That(EosGuestService.DescribeFailure(new Exception("secret-body")), Does.Not.Contain("secret-body"));
            Assert.That(EosGuestService.DescribeFailure(new TimeoutException()), Does.Contain("time"));
        }
        [TestCase(1)] [TestCase(1156)] [TestCase(1157)] [TestCase(65536)]
        public void ReliablePayloadRoundTripsIncludingOffsetsAndFragmentBoundaries(int count)
        {
            var source = Enumerable.Range(0, count + 7).Select(i => (byte)(i % 251)).ToArray();
            var frames = EosPacket.Encode(new ArraySegment<byte>(source, 7, count), 4, 1).ToArray();
            var receiver = new EosPacket.Receiver();
            ArraySegment<byte> payload = default;
            foreach (var frame in frames.Reverse()) receiver.TryReceive(frame, frame.Length, 4, 0, out payload);
            Assert.That(payload.ToArray(), Is.EqualTo(source.Skip(7).ToArray()));
            Assert.That(receiver.PendingCount, Is.Zero);
            Assert.That(frames.All(f => f.Length <= EosPacket.FrameSize), Is.True);
        }
        [Test]
        public void UnreliableCannotFragmentAndSequencingDropsOldOrDuplicateFrames()
        {
            Assert.Throws<ArgumentException>(() => EosPacket.Encode(new ArraySegment<byte>(new byte[1157]), 2, 1).ToArray());
            var receiver = new EosPacket.Receiver();
            foreach (uint value in new uint[] { uint.MaxValue, 0 })
            {
                var frame = EosPacket.Encode(new ArraySegment<byte>(new byte[] { 9 }), 2, value).Single();
                Assert.That(receiver.TryReceive(frame, frame.Length, 2, 0, out _), Is.True);
                Assert.That(receiver.TryReceive(frame, frame.Length, 2, 0, out _), Is.False);
            }
        }
        [Test]
        public void ReassemblyBoundsExpireAndMalformedOrDuplicateFragmentsCannotAllocateMore()
        {
            var receiver = new EosPacket.Receiver();
            for (uint i = 0; i < 20; i++)
            {
                var frame = EosPacket.Encode(new ArraySegment<byte>(new byte[2000]), 4, i).First();
                Assert.That(receiver.TryReceive(frame, frame.Length, 4, 0, out _), Is.False);
                Assert.That(receiver.TryReceive(frame, frame.Length, 4, 0, out _), Is.False);
            }
            Assert.That(receiver.PendingCount, Is.EqualTo(8));
            Assert.That(receiver.HasExpired(11), Is.True);
            byte[] invalid = new byte[EosPacket.FrameSize]; invalid[0] = 1; invalid[1] = 4;
            Assert.That(receiver.TryReceive(invalid, invalid.Length, 4, 0, out _), Is.False);
            Assert.That(receiver.PendingCount, Is.EqualTo(8));
        }

        [Test]
        public void FutureFourTeamPartiesAndQueueAreDefinedButCannotLaunchPrototype()
        {
            var format = MatchFormat.FourTeamDuos;
            Assert.That(format.TeamCount, Is.EqualTo(4)); Assert.That(format.PlayersPerTeam, Is.EqualTo(2));
            Assert.That(format.TotalPlayers, Is.EqualTo(8));
            var first = new AccountIdentity("eos-guest", "1"); var second = new AccountIdentity("eos-guest", "2");
            var source = new[] { first, second };
            var party = new PartySnapshot("party", first, source, format);
            source[0] = second;
            Assert.That(party.Members[0], Is.EqualTo(first));
            var queue = new MatchmakingRequest(format, party, MatchEntryKind.Queue, "0.1.5");
            Assert.Throws<NotSupportedException>(() => queue.RequirePlayablePrivateRoom());
            Assert.Throws<NotSupportedException>(() => new MatchmakingRequest(format, party, MatchEntryKind.PrivateRoom, "0.1.5").RequirePlayablePrivateRoom());
        }
        [Test]
        public void PartyCannotDuplicateMixPoolsOrOverflowOneTeam()
        {
            var first = new AccountIdentity("guest", "1"); var second = new AccountIdentity("guest", "2");
            Assert.Throws<ArgumentException>(() => new PartySnapshot("party", first, new[] { first, first }, MatchFormat.FourTeamDuos));
            Assert.Throws<ArgumentException>(() => new PartySnapshot("party", first, new[] { first, new AccountIdentity("steam", "2") }, MatchFormat.FourTeamDuos));
            Assert.Throws<ArgumentException>(() => new PartySnapshot("party", first, new[] { first, second }, MatchFormat.Duel));
            Assert.Throws<ArgumentException>(() => new PartySnapshot("party", first, new[] { second }, MatchFormat.FourTeamDuos));
            Assert.That(MatchFormat.PoolFor(new AccountIdentity("steam-test", "1")), Is.Not.EqualTo(MatchFormat.PoolFor(new AccountIdentity("steam", "1"))));
            var solo = new PartySnapshot("solo", first, new[] { first }, MatchFormat.Duel);
            Assert.DoesNotThrow(() => new MatchmakingRequest(MatchFormat.Duel, solo, MatchEntryKind.PrivateRoom, "0.1.5").RequirePlayablePrivateRoom());
        }
        [Test]
        public void FourDuosAllocateIntactToFourDistinctTeamsAndOverflowIsAtomic()
        {
            var seats = new TeamSeatAllocator(MatchFormat.FourTeamDuos);
            for (ulong team = 0; team < 4; team++)
            {
                Assert.That(seats.TryAssignParty(new[] { team * 2, team * 2 + 1 }), Is.True);
                Assert.That(seats.TryGet(team * 2, out var first), Is.True);
                Assert.That(seats.TryGet(team * 2 + 1, out var second), Is.True);
                Assert.That(first.Team, Is.EqualTo(team)); Assert.That(second.Team, Is.EqualTo(first.Team));
                Assert.That(first.Seat, Is.Not.EqualTo(second.Seat));
            }
            Assert.That(seats.Count, Is.EqualTo(8));
            Assert.That(seats.TryAssignParty(new ulong[] { 8, 9 }), Is.False);
            Assert.That(seats.Count, Is.EqualTo(8));
            seats.Remove(0); seats.Remove(2); // One seat on two teams is not enough for an intact duo.
            Assert.That(seats.TryAssignParty(new ulong[] { 8, 9 }), Is.False);
            Assert.That(seats.Count, Is.EqualTo(6));
            seats.Remove(1);
            Assert.That(seats.TryAssignParty(new ulong[] { 8, 9 }), Is.True);
        }
        [Test]
        public void SeatAssignmentRejectsDuplicateOrExistingPlayersAndResetClearsReservations()
        {
            var seats = new TeamSeatAllocator(MatchFormat.Duel);
            Assert.That(seats.TryAssignParty(new ulong[] { 7, 7 }), Is.False);
            Assert.That(seats.TryAssignParty(new ulong[] { 7 }), Is.True);
            Assert.That(seats.TryAssignParty(new ulong[] { 7 }), Is.False);
            Assert.That(seats.Count, Is.EqualTo(1));
            seats.Clear();
            Assert.That(seats.Count, Is.Zero);
            Assert.That(seats.TryAssignParty(new ulong[] { 7 }), Is.True);
            seats.TryGet(7, out var seat); Assert.That(seat.Team, Is.Zero);
        }
    }
}
