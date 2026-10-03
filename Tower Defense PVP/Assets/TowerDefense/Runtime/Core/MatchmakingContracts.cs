using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense.Core
{
    public enum MatchmakingPool { Steam, SteamDevelopment, Guest }
    public enum MatchEntryKind { PrivateRoom, Queue }

    public sealed class MatchFormat
    {
        public string Id { get; }
        public int TeamCount { get; }
        public int PlayersPerTeam { get; }
        public int MaximumPartySize => PlayersPerTeam;
        public int TotalPlayers => TeamCount * PlayersPerTeam;
        public bool Playable { get; }
        public MatchFormat(string id, int teams, int playersPerTeam, bool playable = false)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 32 || teams < 2 || teams > 8 || playersPerTeam < 1 || playersPerTeam > 4)
                throw new ArgumentException("Invalid match format.");
            Id = id; TeamCount = teams; PlayersPerTeam = playersPerTeam; Playable = playable;
        }
        public static readonly MatchFormat Duel = new MatchFormat("duel", 2, 1, true);
        public static readonly MatchFormat FourTeamDuos = new MatchFormat("four-team-duos", 4, 2);
        public static MatchmakingPool PoolFor(AccountIdentity account)
        {
            switch (account.Provider)
            {
                case "steam": return MatchmakingPool.Steam;
                case "steam-test": return MatchmakingPool.SteamDevelopment;
                case "guest": case "eos-guest": return MatchmakingPool.Guest;
                default: throw new ArgumentException("Unsupported matchmaking identity.");
            }
        }
    }

    // Immutable party snapshot/queue ticket input, not a networked party or a working queue.
    // Queue admission must later verify every member's authentication/consent on the service.
    public sealed class PartySnapshot
    {
        public string Id { get; }
        public AccountIdentity Leader { get; }
        public IReadOnlyList<AccountIdentity> Members { get; }
        public MatchmakingPool Pool { get; }
        public PartySnapshot(string id, AccountIdentity leader, IEnumerable<AccountIdentity> members, MatchFormat format)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 64 || format == null || !leader.IsValid || members == null)
                throw new ArgumentException("Invalid party.");
            var roster = members.ToArray();
            Pool = MatchFormat.PoolFor(leader);
            if (roster.Length < 1 || roster.Length > format.MaximumPartySize || !roster.Contains(leader)
                || roster.Distinct().Count() != roster.Length || roster.Any(a => !a.IsValid || MatchFormat.PoolFor(a) != Pool))
                throw new ArgumentException("A party needs distinct, same-pool members and must fit one team.");
            Id = id; Leader = leader; Members = Array.AsReadOnly(roster);
        }
    }

    public sealed class MatchmakingRequest
    {
        public MatchFormat Format { get; }
        public PartySnapshot Party { get; }
        public MatchEntryKind Entry { get; }
        public string Version { get; }
        public MatchmakingRequest(MatchFormat format, PartySnapshot party, MatchEntryKind entry, string version)
        {
            if (format == null || party == null || party.Members.Count > format.MaximumPartySize
                || !Enum.IsDefined(typeof(MatchEntryKind), entry) || string.IsNullOrWhiteSpace(version) || version.Length > 32)
                throw new ArgumentException("Invalid matchmaking request.");
            Format = format; Party = party; Entry = entry; Version = version;
        }
        public void RequirePlayablePrivateRoom()
        {
            if (!Format.Playable || Entry != MatchEntryKind.PrivateRoom)
                throw new NotSupportedException("Queues and multi-team matches are planned, not enabled in this prototype.");
        }
    }
}
