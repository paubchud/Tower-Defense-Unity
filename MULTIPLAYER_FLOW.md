# Multiplayer flow and match policy

Status: owner-approved design direction, 2026-10-03 America/Chicago. This is a specification, not shipped functionality. Build 0.1.5 supports code-selected 1v1 rooms; queues, online parties, reconnection, abandonment results and compensation buffs are not implemented. No Milestone 2 gameplay implementation starts in this increment.

## Menu to match

1. Startup automatically uses valid configured Steam, otherwise offers explicit Play as Guest before the main menu. Steam, Steam-test and EOS guest pools remain separate; no cross-play.
2. Play opens mode selection and optional party management. A solo player is a party of one. Invite friends into a persistent party; all members must use compatible versions and the same provider pool. Leaving a match does not itself dissolve the party.
3. Every player chooses their class and starter build/loadout **before queueing**. The authority validates unlocked choices, equipment compatibility and sidegrade rules. Real ownership/progression validation is future work, not supplied by today's local profile.
4. Every party member confirms their selection and consent to queue. The leader submits one whole-party queue ticket. Changing a class/build, mode or party roster cancels the ticket for everyone and requires renewed confirmation; it cannot silently alter an existing ticket.
5. Matchmaking finds a complete compatible roster, keeps each party on one team and fills remaining seats with solos/other fitting parties. It reserves seats atomically; a player cannot occupy two tickets/matches. Partial or stale assignments do not start matches.
6. A match-specific staging lobby shows teams, chosen classes/builds and connection/ready states. **Every player must explicitly ready up**; the party leader cannot ready for friends. Class/build choices are locked for this assignment. To change them, leave the staging lobby and select again.
7. Start only when the full roster is present, choices/maps are validated and everyone is ready. Then all load the selected authored map; gameplay begins only after the required loading acknowledgements and a synchronized start. Ready is not proof that a player finished loading.
8. Match results resolve once. Offer Return to Party/Menu or queue again with fresh individual consent. Requeueing is not automatic punishment or silent enrolment. Private friend challenges retain a separate room-code route.

## Modes and party sizes

| Mode | Teams | Players per team | Total | Queue party size |
| --- | --- | --- | --- | --- |
| 1v1 | 2 | 1 | 2 | Solo only; use a private challenge to duel a friend |
| 2v2v2v2 | 4 | 2 | 8 | Solo or a two-person party |
| 4v4 | 2 | 4 | 8 | Solo or parties of two, three or four |

Parties must fit entirely on one team and are never split onto opposing teams. Reject an oversized party with a clear explanation instead of silently splitting it. All three modes share class/build selection, queue, staging and reconnect boundaries, but each needs its own compatible authored map and gameplay rules.

Only the duel currently runs. The 0.1.5 four-team-duos format/party contracts are gated foundations; **4v4 is newly specified here, not added to the runtime format catalog**. Team castles/lanes, resource ownership, sending targets, ally targeting, group-tower access and victory/elimination rules still need design before either team mode is enabled. Do not assume today's two-side map is an eight-player map.

## How matchmaking forms matches

- Hard compatibility gates: authenticated provider pool, game/protocol/content version, selected mode, available map/ruleset, party size, one active ticket per player and acceptable connectivity/region. Never merge Steam and guest tickets.
- Search oldest compatible tickets first, pack whole parties into teams and avoid starving solos or less-common party sizes. Prefer comparable premade composition and measured latency; exact region thresholds, skill ratings and search widening are **TBD**. These preferences must not override hard gates.
- Team assignment and authority/host selection are separate decisions. Do not always make the first ticket the host; choose a suitable reachable authority and handle simultaneous matchmakers/duplicate assignments without splitting the roster. The actual queue coordination algorithm/service is not selected or implemented; no paid infrastructure is authorized.
- Snapshot each player's provider identity, party origin, selected build, mode, match ID and assigned team/seat. Keep this roster distinct from transient Netcode client IDs. Authenticate admissions rather than accepting an arbitrary client-submitted party snapshot.
- A team is **fully premade** only if every team member came from the same pre-queue party. This was explicitly confirmed by the owner. A party of two in 4v4 that receives two other players is a mixed/random-filled team, even if its members become friends afterward. Snapshot the classification at assignment; later invites, party changes or disconnects cannot change leaver-policy eligibility.
- If a ticket is canceled, a member disconnects during search or an assignment expires, release it for the entire party. In the staging/loading phases, do not begin shorthanded: allow a bounded recovery window or abort the assignment cleanly. Ready/loading timeout lengths and any repeated pregame-dodge policy are TBD. Pregame cancellation is not a 1v1 match forfeit or a reason to grant an abandonment buff.

## Reconnect identity and state

The target lifecycle is Connected -> Disconnected/ReconnectPending -> Reconnected, or Abandoned when its authoritative deadline expires. Hero Alive/Ghost/Respawning is a separate lifecycle: disconnecting must not grant ghost permissions, erase death timers or restore health.

- On network loss or deliberate leave during an active match, reserve the original authenticated identity, team and seat for the reconnect window. Stop new input and transactions from that connection. Match time continues; exact unattended hero behavior/vulnerability must be settled before implementation. Existing towers/sent units continue under the ordinary match rules, without a free replacement hero or extra starting resources.
- The startup screen prioritizes **Rejoin Match** while an eligible reservation exists. Reauthenticate the same provider identity and use a short-lived match-bound admission credential, not a room code, display name or local profile GUID as proof. A copied client cannot claim another player's slot.
- Rejoining may receive a new transport/Netcode ID. Bind it to the original match player, remove stale control and restore authoritative health, life/respawn state, class/build, inventory, currencies, towers/land and cooldowns. Never award starter resources again or replay already accepted transactions/results.
- Keep deadlines on authority time; repeated reconnect attempts cannot extend them indefinitely. A successful return before expiry restores control only after state synchronization. Reject an expired/finished-match reservation and show the outcome clearly.
- Leaving a match does not automatically disband the social party. Expiring a match slot is not deleting an account/profile. Do not fill abandoned active-match seats with queued strangers in this design.

## Active-match leaving rules

| Situation | Grace and expiry | Compensation | Extra leaver penalty |
| --- | --- | --- | --- |
| 1v1 disconnect/leave | **60 seconds to rejoin**; otherwise the opponent wins by forfeit | No team buff | No additional account penalty has been specified; a loss is the match result |
| Fully premade team | Reconnect opportunity; its duration/expiry rules are TBD. Team can still lose/forfeit | **None** | **None**, per owner confirmation |
| Mixed/random-filled team | Reconnect opportunity; if it expires, remove the absent player from active match participation | Small balance buff for the remaining team, only **after confirmed abandonment** | Leaver-policy consequence is intended; its exact type/duration is TBD |

The no-penalty/no-buff rule refers to a team fully formed by its party **before queueing**, not only to everyone quitting together. It does not erase a loss, grant a win/refund or protect a castle. In 1v1 the explicit 60-second forfeit takes precedence over the fact that a solo player is technically a party of one.

Team-mode reconnect durations are not confirmed. A first-test proposal is 60 seconds, but it is not an approved rule. Fully abandoned teams need an elimination/forfeit rule; if no connected player remains, there is nobody to receive a compensation buff. If both 1v1 players disappear, handling of double abandonment/no-contest versus losses is TBD. Normal castle defeat or another valid match end can resolve before a reconnect deadline; never finalize two different results.

### Compensation guardrails

- Only mixed/random-filled teams qualify. Use the frozen assignment classification; disconnecting a premade teammate must not be a way to claim full-premade status afterward and evade the policy.
- No compensation for a brief disconnect during grace, pregame cancellation, ordinary hero death or an ineligible fully premade team. On timeout, remove the player's active match slot/control; late rejoin is rejected under the mixed-team rule.
- Keep one authoritative, visible team modifier with a cap and explicit stacking/missing-seat rules. Target stat(s), strength, scaling and eligible duration are TBD. Do not choose an arbitrary damage/health/income increase before combat/economy measurements exist.
- The buff must compensate modestly, not make losing a teammate a stronger strategy than retaining one. Avoid repeated leave/rejoin stacking, permanent reward bonuses, asset duplication/redistribution and buffs surviving results/rematches. Test coordinated intentional abandonment and uneven team sizes.
- In a mixed team, the specified modifier benefits the remaining team, including its small-party members. Measure whether a coordinated party member abandoning creates a leave-to-win strategy. Any later restriction on eligible survivors needs an explicit policy decision; do not silently exclude teammates or promise abuse-proof fairness.
- No report/ranked punishment backend or reliable ban-evasion protection is implemented. Guest identity persistence is not proof that an account can never be reset; public competitive rewards and punitive policy need trusted processing before launch.

## Authority survival is a required implementation gate

Today's Steam/EOS matches are player-hosted and have no host migration, durable live-match snapshot or trusted result service. If that host quits/crashes, the relay/lobby does **not** preserve the running game for the remaining clients. Therefore today's adapter cannot promise every player's 60-second return or a trustworthy opponent-win record after host loss.

Before enabling the proposed rules, choose an authority-survival strategy: recoverable host state/verified migration or an appropriate server/service that can preserve the match and finalize results independently. Evaluate the owner's no-usage-billing constraint; do not enable paid services or claim EOS relay is a gameplay server. Test host loss separately from ordinary client disconnection. This is a required architecture decision, not an already solved backend feature.

## Implementation order and acceptance

1. Before Milestone 2: record this design and freeze player/profile/team/seat/build identities; finish separate-device guest/Steam and real Mac tests. Address the known same-identity error/role-aware cleanup in a separately authorized runtime increment.
2. During 1v1 combat/economy work: key owned gameplay state by stable match-player identity; separate leave, death and despawn; make transactions/results idempotent and snapshots complete enough to recover. Do not replace the state owner whenever a transport ID changes.
3. Before public queue play: implement persistent parties/consent, private-vs-queue entry, cancelable authenticated tickets, fair whole-party admission, the all-ready/full-loaded staging gate and version/pool compatibility. Start with 1v1; test sparse traffic, contention, cancellation and bad actors.
4. Before promising reconnect/forfeit policy: implement authority survival, reserved slots, reauthentication, state resynchronization, authoritative deadlines, single result finalization and explicit abandoned-player UI. Test both host roles, deliberate leave, crash, network loss, deadline races, repeated retries and both players leaving.
5. Before team queues: add 2v2v2v2 and 4v4 format/map/rules, original party classification, team grace/abandonment, selected compensation/penalty policy and measured eight-player simulation/network budgets. Test solo, partial and full parties, entire-team loss, uneven teams and compensation exploits.

This sequence prepares the architecture without enabling an unfinished mode or delaying every private 1v1 combat experiment until a full matchmaking service exists. Public queue/reconnect claims require their own acceptance gates.
