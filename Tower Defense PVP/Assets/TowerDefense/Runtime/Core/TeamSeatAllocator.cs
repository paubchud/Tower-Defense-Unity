using System;
using System.Collections.Generic;

namespace TowerDefense.Core
{
    public readonly struct TeamSeat
    {
        public readonly int Team, Seat;
        public TeamSeat(int team, int seat) { Team = team; Seat = seat; }
    }

    // Per-match transport IDs only, never save/profile IDs. Reserve a complete party
    // atomically on one team; future queue admission supplies verified party membership.
    public sealed class TeamSeatAllocator
    {
        private readonly MatchFormat format;
        private readonly Dictionary<ulong, TeamSeat> seats = new Dictionary<ulong, TeamSeat>();
        public int Count => seats.Count;
        public TeamSeatAllocator(MatchFormat format) { this.format = format ?? throw new ArgumentNullException(nameof(format)); }
        public bool TryGet(ulong player, out TeamSeat seat) => seats.TryGetValue(player, out seat);
        public bool TryAssignParty(IReadOnlyList<ulong> party)
        {
            if (party == null || party.Count == 0 || party.Count > format.MaximumPartySize) return false;
            var unique = new HashSet<ulong>();
            foreach (ulong player in party) if (!unique.Add(player) || seats.ContainsKey(player)) return false;
            for (int team = 0; team < format.TeamCount; team++)
            {
                var free = new List<int>();
                for (int slot = 0; slot < format.PlayersPerTeam; slot++)
                {
                    bool occupied = false;
                    foreach (var seat in seats.Values) if (seat.Team == team && seat.Seat == slot) { occupied = true; break; }
                    if (!occupied) free.Add(slot);
                }
                if (free.Count < party.Count) continue;
                for (int i = 0; i < party.Count; i++) seats.Add(party[i], new TeamSeat(team, free[i]));
                return true;
            }
            return false;
        }
        public void Remove(ulong player) => seats.Remove(player);
        public void Clear() => seats.Clear();
    }
}
