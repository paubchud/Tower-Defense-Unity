# Tower Defense PVP - Development Plan

This is the living implementation sequence. See [GAME_DESIGN.md](GAME_DESIGN.md) for the gameplay specification, assumptions, and unresolved rules. Update both documents when a design decision changes.

## Current state

- Unity project: `Tower Defense PVP`, Unity `6000.6.3f1`, Universal Render Pipeline.
- Repository contains the starter project with Unity-generated folders ignored.
- No custom gameplay or multiplayer systems have been implemented as part of this planning work.
- The original generic wave-defense plan has been revised around classes, player-sent units, three currency roles, land, materials, and invasions.

## Target and sequence

Build one small 1v1 map with two straight lanes. Prove two-client networking early, then add combat, economy, harvesting, and invasion in small playable milestones.

Brief local experiments are useful, but do not build the entire game locally before introducing multiplayer. Each milestone must work with two players before the next dependent milestone begins.

| Milestone | Build | Acceptance gate |
| --- | --- | --- |
| 0. Foundation | Rules, map schema, class schema, project structure, authority model | Dependencies and first design choices recorded |
| 1. Network smoke test | Menu, class selection, test arena, two connected heroes | Both clients agree on player identity, selected class, movement, and match reset |
| 2. Sending and defense | Sent units, castle damage, one tower, XP upgrade, gold rewards | Complete a match with matching health/resources on both clients |
| 3. Land and economy | Purchase land, harvest one material, pay tower costs, hero leveling | Simultaneous actions cannot duplicate rewards or overspend |
| 4. Invasion | Cross into enemy territory, fight, steal, die/return | Both players can invade and recover under the agreed rules |
| 5. Complete prototype | Full menu-to-results loop, HUD, rematch, one optional challenge | Two standalone clients complete repeated matches within measured budgets |
| 6. Expansion | Store progression, more classes/content/maps, automation, online services | New content uses the established systems and passes balance/performance gates |

## Milestone 0 - Foundation

1. Decide view, controls, target platform, and initial hero interaction rules.
2. Choose a networking solution compatible with this project and define the local test setup. Verify package documentation and compatibility at implementation time.
3. Use server authority for gameplay. A host is a possible private prototype deployment; gameplay systems should refer to an authority interface rather than assuming a particular player is always the host.
4. Establish folders and small assemblies for game rules, definitions, networking, presentation/UI, and tests as needed. Avoid creating speculative systems before a milestone needs them.
5. Define minimal data for classes, loadouts, units, towers, land plots, nodes, and authored maps. Add fields only when needed.
6. Separate persistent loadout ownership from temporary match state from the start. Persistence itself can wait.
7. Establish bootstrap/menu and match lifecycles, including cleanup/reset ownership.

Gate: record these choices and open the project without errors before implementing the network smoke test.

## Milestone 1 - Menu, classes, and network smoke test

1. Build Main Menu with Play, Store, and Exit. Store starts as a placeholder; define its persistent-currency role in the UI.
2. Play opens Warrior/Wizard selection and a minimal lobby with ready state. Use default free prototype loadouts.
3. Build an authored test arena with two straight lanes, two castles, plot markers, resource locations, and space for invasion. Movement connectivity depends on the selected invasion rules.
4. Represent each lane as map path data with stable IDs, endpoints, and distance along the path.
5. Connect two players and assign sides. Spawn their selected heroes with basic movement and class presentation.
6. Synchronize selected class, player identity, movement, readiness, and match state.
7. Test host plus client, then two standalone processes and, when available, two machines.

Gate: both players see correct class/side assignment, can move, and can leave/restart without duplicate heroes or stale subscriptions.

## Milestone 2 - Sending, defense, and castle victory

1. Define the cost or cooldown for sending, XP award timing, kill reward owner, and unit upgrade scope.
2. Implement one sendable unit type. An accepted server command creates units on the opponent's lane and awards XP according to the selected rule.
3. Implement authoritative path movement, health, damage, death, and arrival at a castle.
4. Add one basic attack per hero and one tower with a simple target/fire loop. For this milestone, start each side with a preset tower; purchasing comes next.
5. Reward defender gold exactly once per eligible kill. Keep match XP and gold as separate balances.
6. Implement one XP-funded sending-unit upgrade. Show its cost and effect in the HUD.
7. End the match on castle destruction under the agreed damage rule and support a clean reset.

Gate: two players send, defend, upgrade, and finish a match with identical authoritative results. Validate rejected sends, duplicate commands, simultaneous kills, and match-end command rejection.

## Milestone 3 - Land, harvesting, and hero growth

1. Decide gold-funded hero leveling, initial resource/land values, tower recipe, and node depletion/recovery rules.
2. Add predefined purchasable plots and ownership checks. Use a small number of building slots initially if the design accepts them.
3. Add one manually harvested material. Validate proximity, node yield, action time, and inventory capacity on the server.
4. Build the tower from Milestone 2 on owned land using materials. Validate slots, lane clearance, and the full cost before changing state.
5. Add one gold-funded hero level/improvement using the selected progression rule. Reset it each match.
6. Give the player clear feedback for costs, occupied plots, empty nodes, and invalid actions.

Gate: two players can harvest and transact simultaneously without duplicated materials, negative balances, double-owned plots, or partial purchases. The main loop remains viable when a player loses a fight or spends poorly.

## Milestone 4 - Invasion

1. Agree on territory access, attackable targets, theft source/type/limits, death, respawn, and return rules.
2. Implement hero crossing or the agreed entrance mechanic. Territory ownership, damage permissions, and action permissions must be explicit.
3. Allow hero combat and only the target categories selected for the prototype.
4. Implement one resource theft interaction. Remove resources from the source and credit the destination in a single authoritative operation.
5. Add death and respawn/return. Release targets, harvesting actions, and temporary interactions when a hero dies or disconnects.
6. Test attacking while the home lane is under pressure and recovery after a failed invasion.

Gate: invading is useful but carries an opportunity cost. No resource duplication, wrong-side building, repeated death rewards, or permanent loss of player control.

## Milestone 5 - First complete playable prototype

1. Connect Main Menu, Store placeholder, class selection, lobby, match, results, and rematch.
2. Add a readable HUD for castle health, hero state, gold, XP, materials, and sending/building actions.
3. Finish essential prompts, disconnect behavior, and match reset. Select a simple prototype disconnect policy; reconnect can follow later.
4. Optionally add one authored neutral challenge once the required economy and invasion features are stable.
5. Measure a standalone development build under normal and stress conditions. Establish provisional caps for units, towers, effects, and resource production from the results.
6. Test delay, packet loss where supported, simultaneous actions, repeated matches, and both host/client roles.
7. Save a focused commit and a reproducible internal build with play/test instructions.

Definition of done: two players can select Warrior/Wizard, send units, spend XP, earn/spend gold, harvest, build on bought land, invade, win/lose, and rematch on the small map. A fresh match resets all temporary growth while keeping the chosen starter loadout. Real persistent purchases are not required for this prototype.

## Milestone 6 - Expansion order

1. Add starter equipment alternatives and a small Store using a versioned local profile for private tests. Keep persistent unlocks separate from match strength; validate tradeoffs with playtests.
2. Add more classes, abilities, sent units, unit upgrades, and towers through their shared definitions and systems. Verify balance and load at every addition.
3. Add authored maps and challenges with the existing path/plot/node schema. Test invasion routes and resource access for fairness.
4. Add automatic harvesting as a match investment using the existing harvest transactions. Bound output and schedule work without an update loop per node.
5. Add room codes or matchmaking, reconnect/rematch improvements, and additional supported platforms as required.
6. Before public persistent rewards or ranked play, add trusted match result processing, backend/profile validation, and an appropriate server deployment. A player-hosted server can manipulate its own authoritative state; client validation alone does not protect competitive rewards.
7. Add more content, accessibility/settings, tutorials, art/audio/VFX polish, and progression tuning once the core systems have measured headroom.

## Architecture rules that protect later work

- Keep rules separate from networking, rendering, UI, and audio. The same rules should run in local tests and on the match authority.
- Use data assets for reusable definitions; never store a player's changing balances, health, or cooldowns in shared definition assets.
- Every networked entity and player has stable identity and explicit ownership. Validate actions by identity, range, state, and permissions on the server.
- Route send, upgrade, harvest, build, level, and steal requests through explicit authoritative transactions. Each request either succeeds completely or leaves state unchanged.
- Reward each kill, send, harvest, and match result once. Choose per-command sequence or equivalent duplicate protection where retries can repeat a transaction.
- Keep permanent currency, match XP, gold, and materials distinct. Use integer amounts and reject invalid costs, overflows, and overspending.
- Use a shared combat/damage layer with explicit target permissions for heroes, units, castles, towers, and future harvesters.
- Author paths, plots, nodes, castles, and challenge markers as map data. Avoid hard-coded scene-object lookups and assumptions that only the first map exists.
- Simulation follows server time. Rendering can interpolate; gameplay costs, cooldowns, harvest durations, and deaths must not depend on client frame rate.
- Define spawn/despawn, connection, death, and rematch cleanup in each system. Cancel outstanding actions and unregister objects during cleanup.
- Add interfaces and services when they have a concrete consumer. Do not prebuild an entire account, inventory, crafting, or matchmaking framework for the prototype.

## Performance and network guardrails

- Track CPU/GPU frame times, allocations, memory, entity counts, bandwidth, and server simulation time in standalone development builds.
- Set a target frame rate and test machine after the platform decision. Define load ceilings from measurements before expanding enemy density or content.
- Pool repeated units, projectiles, and effects when introducing them. Verify pooled state resets and correct network spawn/despawn behavior.
- Centralize or stagger targeting and resource updates; use local/spatial candidate sets as counts grow. Avoid every tower searching every unit every rendered frame.
- Follow fixed lane progress for sent units; do not add per-unit navigation work unless map behavior requires it.
- Do not build custom prediction/rollback or optimization frameworks before measurement shows a need. Choose movement replication suited to the selected controls and transport.
- Replicate shared gameplay state at appropriate rates; create cosmetic audio and effects locally from events where practical. Use interest filtering only when the map/entity load justifies it.
- Cap active sends, spawn rate, projectile/effect counts, and automated resource output so neither long matches nor repeated inputs grow work without limit.
- Schedule harvest and cooldown work from server time. Avoid allocations in repeated targeting, movement, and message handling.
- Maintain an opt-in debug overlay with connections, latency, active counts, and economy totals; disable verbose diagnostics in release builds.

## Verification at each milestone

- Project compiles and the affected feature works in both player roles.
- Server and clients agree on health, ownership, currency, inventory, and outcome.
- Invalid, late, or repeated commands cannot grant resources or apply damage twice.
- Gameplay stays usable with measured network delay and at the milestone's target load.
- Death, disconnect, match end, and rematch release subscriptions, targets, pooled objects, and pending interactions.
- Match reset does not erase persistent unlocks or carry over temporary growth.
- Profile the added workload, address regressions, and commit a focused, working increment.

## Next action

Before implementation, settle view/controls/platform and the first connection setup. Then begin Milestone 0 followed by the two-player smoke test. Decide economy and invasion details before their respective milestones.
