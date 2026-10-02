# Tower Defense PVP - Development Plan

This is the living implementation sequence. See [GAME_DESIGN.md](GAME_DESIGN.md) for the gameplay specification, assumptions, and unresolved rules. Update both documents when a design decision changes.

## Current state

- Unity project: `Tower Defense PVP`, Unity `6000.6.3f1`, Universal Render Pipeline.
- Repository contains the Unity project with generated caches, logs, and builds ignored.
- Foundation and the first network-smoke implementation are now present under `Assets/TowerDefense`: menu/class selection, ready lobby, two straight lanes, 3D blockout heroes, server-authoritative movement, hero-centered orbit cameras, starter hotbar, and management-panel placeholders. See README.md for play/build instructions and the implemented-versus-planned boundary.
- The original generic wave-defense plan has been revised around classes, player-sent units, three currency roles, land, materials, and invasions.
- Confirmed presentation/controls: 3D models, continuous hero-centered camera follow, and WASD hero movement. Start from an elevated/top-down angle and interpret right-click dragging as orbiting around the hero; camera tuning can change during development.
- Confirmed death rule: timed respawn with a controllable ghost that can roam and buy/sell/interact with its own side, but cannot attack or collect resources.
- Future-system requirements: scrollable weapon/item hotbar, shortcuts for match management panels, configurable class energy pools, and equipment slots with class-specific starter gear. Loot/crafting/upgrading rules remain undecided.
- Roster target: five technology groups with five classes each (25 total). Warrior and future bow-wielding Hunter are Primate; Wizard is Mystic. Classes in the same group share its tower catalog. Three group names and the remaining roster are not yet supplied.

## Target and sequence

Build one small 1v1 map with two straight lanes. Prove two-client networking early, then add combat, economy, harvesting, and invasion in small playable milestones.

Brief local experiments are useful, but do not build the entire game locally before introducing multiplayer. Each milestone must work with two players before the next dependent milestone begins.

## First implementation defaults

- Target: Windows private prototype. Unity Netcode for GameObjects 2.13.3 with Unity Transport; one player hosts, another joins by address/port. MainMenu and TestArena load locally before connection; no matchmaking, relay, persistent rewards, or dedicated deployment yet.
- Server owns hero movement and replicated class/side/ready/selected-item state. Input is sent at up to 30 Hz; cameras are local. A third connection is rejected. Losing the opponent returns the remaining player to the ready lobby.
- Starter gear: Warrior sword/mail, Wizard staff/robes; both have a pickaxe. Three provisional hotbar slots (weapon, tool, empty), wheel wrapping, and B/U/T/I/Esc panel shortcuts. Larger inventories, acquisition, crafting, compatibility rules, and rebinding UI wait for their consumers.
- Energy assets: Warrior stamina and Wizard mana, provisionally 100 capacity. Generic pool spending/recovery rules are independently tested, but combat costs and networked pool state are not connected yet. Equipment UI reports definitions, not a live energy HUD.
- Two shared tower catalogs contain definition placeholders only. No functioning tower or troop is implied by those assets.
- The map currently allows crossing between sides to check movement/connectivity; this does not settle final invasion permissions, theft, or targets. Aim/attack/harvest controls remain undecided until their milestone.
- Windows build and automated validation output stay outside Assets in ignored `Builds`; tests live under `Assets/TowerDefense/Tests/Editor`.

## Verification record - first network slice, 2026-10-01

- Unity EditMode: 6 tests passed, 0 failed. Checks cover catalog references, shared group catalogs, starter-container isolation/hotbar wrapping, invalid/overspent energy costs, recovery limits, and lane/spawn bounds.
- Windows development build completed successfully. Two standalone processes passed class/side/movement/item synchronization, hero-centered orbit, camera-relative WASD, wheel selection, all five panel shortcuts, and blocked movement/orbit/item scrolling during panels.
- Both peers passed reset-to-lobby and re-ready, leave/menu cleanup, and a new session with swapped classes. Host disconnect cleanup passed. A third client was rejected while the accepted players continued.
- Live menu, arena, and equipment views were rendered to PNGs and visually checked for both roles. Art is intentionally primitive 3D blockout geometry. See README's known unused-postprocessing warning.
- Repeatable runner: `Tools/ValidatePrototype.ps1 -Capture`. Latest successful output is in ignored `Builds/Validation/Smoke-20261001-185551`; generated artifacts are not part of the source commit.
- Not yet tested: editor plus standalone together, two physical machines, internet connections, latency/loss, high entity counts, or a complete tower-defense match. Do not infer a performance or public-network guarantee from the localhost smoke test.

| Milestone | Build | Acceptance gate |
| --- | --- | --- |
| 0. Foundation | Class/group/item/resource definitions, map schema, input actions, authority model | Dependencies and first design choices recorded |
| 1. Network smoke test | Menu, two classes, 3D arena, WASD, local cameras, starter inventory/hotbar and shortcuts | Player/selection state agrees; cameras and UI input are independent |
| 2. Sending and defense | Sent units, one tower per initial group, class attacks/energy, XP/gold, ghost respawn | Combat, energy, group tower access, and respawn agree on both clients |
| 3. Land and economy | Buy land, harvest, build/trade, hero leveling, inventory and ghost permissions | Transactions cannot duplicate items/resources or overspend; ghosts cannot collect |
| 4. Invasion | Cross into enemy territory, fight, steal, return or respawn | Both players can invade and recover under the agreed ghost/respawn rules |
| 5. Complete prototype | Full menu-to-results loop, HUD, rematch, one optional challenge | Two standalone clients complete repeated matches within measured budgets |
| 6. Expansion | Class unlocks, five groups/about 25 classes, gear acquisition/crafting, maps, automation, services | New content uses the established systems and passes balance/performance gates |

## Milestone 0 - Foundation

1. Use 3D models, WASD, and continuous hero-centered camera follow. Start from an elevated angle with right-click drag orbit. Define named actions for hotbar selection, match shop, upgrades, troop-sending UI, and inventory/equipment. Decide aiming, bindings, target platform, and initial interaction rules; tune the camera in the prototype.
2. Choose a networking solution compatible with this project and define the local test setup. Verify package documentation and compatibility at implementation time.
3. Use server authority for gameplay. A host is a possible private prototype deployment; gameplay systems should refer to an authority interface rather than assuming a particular player is always the host.
4. Establish folders and small assemblies for game rules, definitions, networking, presentation/UI, and tests as needed. Avoid creating speculative systems before a milestone needs them.
5. Define minimal data for classes, technology groups/tower catalogs, loadouts, energy types/pools, items/equipment slots, units, towers, plots, nodes, and maps. Establish stable IDs and references; implement only the fields and runtime behavior consumed by the first two classes and current milestone.
6. Separate persistent loadout ownership from temporary match state from the start. Persistence itself can wait.
7. Establish bootstrap/menu and match lifecycles, including cleanup/reset ownership.

Gate: record these choices and open the project without errors before implementing the network smoke test.

## Milestone 1 - Menu, classes, and network smoke test

1. Build Main Menu with Play, Store, and Exit. Store starts as a placeholder; define its persistent-currency role in the UI.
2. Play opens Warrior/Wizard selection and a minimal lobby with ready state. Show their technology groups (Primate/Mystic) and support difficulty/unlock metadata. Use default free prototype loadouts; real unlock progression follows later.
3. Build an authored test arena with two straight lanes, two castles, plot markers, resource locations, and space for invasion. Movement connectivity depends on the selected invasion rules.
4. Represent each lane as map path data with stable IDs, endpoints, and distance along the path.
5. Connect two players and assign sides. Spawn selected 3D heroes with WASD movement, minimal owned starter inventory, equipped-slot references, and class presentation. Choose the two starter loadouts before implementation.
6. Add a local camera controller that continuously follows and focuses on the hero, with an elevated initial angle and right-click drag orbit. Tune viewing limits and obstruction handling. Use the working defaults in GAME_DESIGN.md and keep the controller separate from the hero.
7. Add a small hotbar with mouse-wheel selection and named shortcuts for match shop, upgrades, troop-sending, and inventory/equipment panels. Panels can be placeholders until their gameplay milestone. Keep the Main Menu Store and in-match shop separate.
8. Synchronize selected class, player identity, movement, active item selection where gameplay needs it, readiness, and match state. Derive technology group from the validated class.
9. Check that the hero stays centered while moving or dragging, rotation alone does not move the hero or alter action range, and one player's camera does not affect the other's view. UI focus must prevent accidental orbit, attacks, hotbar selection, or sends; menu scrolling must not also cycle items. Verify camera-relative WASD and map visibility.
10. Test host plus client, then two standalone processes and, when available, two machines.

Gate: both players see correct class/group/side assignment and starter gear, can select hotbar items and open panels, can move with WASD/orbit independent hero-centered cameras, and can leave/restart without duplicate items/heroes or stale subscriptions.

## Milestone 2 - Sending, defense, and castle victory

1. Define sending cost/cooldown, XP award timing, kill reward owner, unit upgrade scope, and the first two classes' energy/cost/recovery rules. Choose one starter tower for each initial group.
2. Implement one sendable unit type. An accepted server command creates units on the opponent's lane and awards XP according to the selected rule.
3. Implement authoritative path movement, health, damage, death, and arrival at a castle.
4. Add one basic attack per hero using the selected weapon and generic energy/cooldown rules. Add one starter tower each for Primate and Mystic using shared targeting/fire code. Start each side with its group's preset tower; purchasing comes next.
5. Reward defender gold exactly once per eligible kill. Keep match XP and gold as separate balances.
6. Implement one XP-funded sending-unit upgrade. Connect the upgrade and troop-sending shortcuts to their panels; show costs/effects in the HUD.
7. End the match on castle destruction under the agreed damage rule and support a clean reset.
8. Add authoritative hero death, controllable ghost movement, and timed respawn. Choose an initial timer/location/health policy before implementation. Keep the camera on the controlled hero/ghost and prevent ghost attack requests.
9. Validate item ownership, selected/equipped weapon, life state, cooldown, and all energy costs together before accepting hero actions. Test an empty pool, rapid item changes, and simultaneous energy spending; inventory selection must not bypass ghost restrictions.

Gate: two players send, defend, upgrade, use valid class equipment/energy, die/roam/respawn, and finish a match with identical authoritative results. Validate group tower access, rejected sends, duplicate commands, simultaneous kills/energy costs, ghost attack rejection, respawn deadlines, and match-end command rejection. The prototype must not leave a player without control during the death timer.

## Milestone 3 - Land, harvesting, and hero growth

1. Decide gold-funded hero leveling, initial resource/land values, tower recipe, node depletion/recovery rules, and one sellable own-side asset/refund policy.
2. Add predefined purchasable plots and ownership checks. Use a small number of building slots initially if the design accepts them.
3. Add one manually harvested material. Validate proximity, node yield, action time, and inventory capacity on the server.
4. Build the selected class's technology-group tower from Milestone 2 on owned land using materials. Derive the build menu from the group's catalog and validate catalog eligibility, slots, lane clearance, and full cost before changing state.
5. Add one gold-funded hero level/improvement using the selected progression rule. Reset it each match.
6. Give the player clear feedback for costs, occupied plots, empty nodes, and invalid actions.
7. Implement one own-side sale/refund transaction and allow the same purchases, sales, and management while alive or in ghost mode. Enforce normal costs, ownership, and placement rules; a refund is distinct from collecting materials.
8. Reject ghost pickup/mining/harvesting requests, cancel in-progress harvesting on death, and ensure dying between request and completion cannot award newly collected materials. Define which passive rewards/automation outputs continue before adding those systems.
9. Route inventory/material purchases, sales, and slot changes through the same authoritative ownership rules. Test occupied/incompatible slots and repeated requests without item duplication. Decide ghost equipping permissions before allowing slot changes while dead; full loot/crafting content follows the prototype.

Gate: two players can harvest and transact simultaneously without duplicated items/materials, negative balances, double-owned plots, wrong-group towers, or partial purchases. Ghosts can manage their own side but cannot gather. Test death during a harvest and respawn during a transaction. The main loop remains viable when a player loses a fight or spends poorly.

## Milestone 4 - Invasion

1. Agree on territory access, attackable targets, theft source/type/limits, return rules, carried-resource handling, and any invasion-specific respawn/ghost roaming limits. Use the established timed respawn and ghost permissions.
2. Implement hero crossing or the agreed entrance mechanic. Territory ownership, damage permissions, and action permissions must be explicit.
3. Allow hero combat and only the target categories selected for the prototype.
4. Implement one resource theft interaction. Remove resources from the source and credit the destination in a single authoritative operation.
5. Integrate invasion with the existing hero death/ghost/respawn lifecycle. Ghosts cannot attack or steal resources. Release combat targets and harvesting actions on death; preserve valid own-side management access while roaming as a ghost. Disconnect cleanup remains separate from death.
6. Test attacking while the home lane is under pressure and recovery after a failed invasion.

Gate: invading is useful but carries an opportunity cost. No resource duplication, ghost theft/damage, wrong-side building, repeated death rewards, or permanent loss of player control. Both clients agree when the ghost timer ends and the living hero returns.

## Milestone 5 - First complete playable prototype

1. Connect Main Menu, Store placeholder, class selection, lobby, match, results, and rematch.
2. Add a readable HUD for castle health, hero/ghost state, respawn countdown, class energy pools, hotbar/equipped items, gold, XP, materials, and sending/building actions. Finish essential panel shortcuts without input conflicts.
3. Finish essential prompts, disconnect behavior, and match reset. Select a simple prototype disconnect policy; reconnect can follow later.
4. Optionally add one authored neutral challenge once the required economy and invasion features are stable.
5. Measure a standalone development build under normal and stress conditions. Establish provisional caps for units, towers, effects, and resource production from the results.
6. Test delay, packet loss where supported, simultaneous actions, repeated matches, and both host/client roles.
7. Save a focused commit and a reproducible internal build with play/test instructions.

Definition of done: two players can select Warrior/Wizard with Primate/Mystic tower access, use starter gear through a scrollable hotbar and the chosen energy rules, open management panels by keybind, send/upgrade units, grow their economy/hero, build on bought land, invade, die into a controllable ghost, manage their own side during a synchronized timer, respawn, finish, and rematch. Ghosts cannot attack or collect resources. Fresh matches reset temporary growth, inventory/equipment acquisitions, and energy state to the selected starter setup. The complete Store/unlock system and loot/crafting content are not required for this prototype.

## Milestone 6 - Expansion order

1. Add class-unlock progression, starter equipment alternatives, and a small Store using a versioned local profile for private tests. Decide currency/prerequisite/mastery requirements, show difficulty/group labels, and keep beginner classes viable against later unlocks.
2. Add Hunter with a bow as an early extension test: new class/weapon behavior, same Primate tower catalog as Warrior. Then introduce one class from a newly named technology group to prove that adding a group/catalog does not require rewriting UI or building rules.
3. Add one simple found-gear interaction after choosing loot ownership/death handling. Decide slot compatibility and equipment effects, then add crafting/upgrades only once acquisition rules, recipes, and ghost permissions are settled. Validate inventory, hotbar, and equipment ownership across each transition.
4. Expand toward all five groups and approximately 25 classes, using supplied class designs. Add class-specific abilities/energies/equipment, group tower sets, sent units, and upgrades through existing definitions. Verify whole-loadout/group balance and performance in small batches.
5. Add authored maps and challenges with the existing path/plot/node schema. Test invasion routes and resource access for fairness.
6. Add automatic harvesting as a match investment using the existing harvest transactions. Bound output and schedule work without an update loop per node.
7. Room-code internet play was brought forward after the first network slice so friends can test remotely. Steam is the primary storefront, with a standalone itch build also planned. Keep authentication/store invites separate from shared gameplay and transport. Add automatic Steam sign-in/friend invites once the game's Steamworks App ID is available, and a shared Play queue when the core match works. Preserve cross-store rooms without port forwarding; add durable account linking before persistent rewards. Add reconnect/rematch improvements and other platforms as required. Relay is not a trusted match-result backend.
8. Before public persistent rewards or ranked play, add trusted match result processing, backend/profile validation, and an appropriate server deployment. A player-hosted server can manipulate its own authoritative state; client validation alone does not protect competitive rewards.
9. Add more content, keybinding/settings UI, accessibility, tutorials, art/audio/VFX polish, and progression tuning once the core systems have measured headroom.

## Future-system dependencies

| System | Establish in the prototype | Expand after the core match works |
| --- | --- | --- |
| Hotbar and shortcuts | Owned-item references, selected slot, named/context-aware input actions | More usable items, rebinding UI, additional panels |
| Class energies | Generic resource IDs, per-class pools/costs, authoritative validation | Additional energy types, equipment modifiers, advanced resource behaviors |
| Equipment | Starter inventory, compatible slots, equip ownership, visual references | Found gear, crafting, upgrades after their rules are chosen |
| Technology groups | Primate/Mystic catalog references; build-menu and server eligibility | Remaining three groups and full shared tower sets |
| Class progression | Class IDs, complexity and unlock metadata, two free test classes | Persistent unlock requirements and the supplied roster, without escalating baseline power |

## Architecture rules that protect later work

- Keep rules separate from networking, rendering, UI, and audio. The same rules should run in local tests and on the match authority.
- Keep the camera controller and input-to-world mapping separate from hero simulation. Use 3D world positions for movement, aim, and placement; changing camera framing must not change authoritative gameplay or reveal hidden state.
- Use data assets for reusable definitions; never store a player's changing balances, health, or cooldowns in shared definition assets.
- Resolve towers from class -> technology group -> shared catalog. Keep class-specific equipment/abilities/energies separate; do not hard-code a 25-class limit, duplicate catalogs per class, or trust a client-submitted group/tower choice.
- Give inventory items/instances, equipment slots, abilities, energy types, and input actions stable IDs. A hotbar or equipped slot references owned state; it must not copy/duplicate items. Recompute equipment-derived values from the equipped set so repeated equips cannot stack modifiers accidentally.
- Use generic per-class energy pools and data-driven costs/recovery; validate all costs and cooldowns before applying an action. Keep energy distinct from XP/gold/materials and settle death/max-capacity behavior before adding energy-changing gear.
- Route input through named actions and UI/gameplay contexts. Reserve wheel selection and right-drag orbit, prevent menu input from also triggering gameplay, and keep local UI selection separate from authoritative equip/use transactions.
- Every networked entity and player has stable identity and explicit ownership. Validate actions by identity, range, state, and permissions on the server.
- Route send, upgrade, harvest, build, level, steal, item-use, equip, trade, and future craft requests through explicit authoritative transactions. Each request either succeeds completely or leaves state unchanged.
- Reward each kill, send, harvest, and match result once. Choose per-command sequence or equivalent duplicate protection where retries can repeat a transaction.
- Keep permanent currency, match XP, gold, and materials distinct. Use integer amounts and reject invalid costs, overflows, and overspending.
- Use a shared combat/damage layer with explicit target permissions for heroes, units, castles, towers, and future harvesters.
- Validate action permissions against the actor's current life state as well as ownership/range. A ghost's blocked combat/collection actions must not disable its side's existing towers or remove allowed buy/sell/management access. Respawn deadlines use server time and cancel cleanly at match end.
- Author paths, plots, nodes, castles, and challenge markers as map data. Avoid hard-coded scene-object lookups and assumptions that only the first map exists.
- Simulation follows server time. Rendering can interpolate; gameplay costs, cooldowns, harvest durations, and deaths must not depend on client frame rate.
- Define spawn/despawn, connection, death, and rematch cleanup in each system. Cancel outstanding actions and unregister objects during cleanup.
- Add interfaces and services when they have a concrete consumer. Implement a minimal starter inventory/slot/resource model for the prototype; defer the full loot, crafting, account, and matchmaking feature sets until their milestones.

## Performance and network guardrails

- Track CPU/GPU frame times, allocations, memory, entity counts, bandwidth, and server simulation time in standalone development builds.
- Set a target frame rate and test machine after the platform decision. Define load ceilings from measurements before expanding enemy density or content.
- Pool repeated units, projectiles, and effects when introducing them. Verify pooled state resets and correct network spawn/despawn behavior.
- Centralize or stagger targeting and resource updates; use local/spatial candidate sets as counts grow. Avoid every tower searching every unit every rendered frame.
- Follow fixed lane progress for sent units; do not add per-unit navigation work unless map behavior requires it.
- Do not build custom prediction/rollback or optimization frameworks before measurement shows a need. Choose movement replication suited to the selected controls and transport.
- Replicate shared gameplay state at appropriate rates; create cosmetic audio and effects locally from events where practical. Use interest filtering only when the map/entity load justifies it.
- Share catalog/definition IDs and update mutable equipment, selection, and energy state as needed. Keep private inventory details visible only to authorized players; send opponents the equipped appearance/gameplay information they need. Avoid sending full catalogs or rebuilding hotbar/equipment UI every rendered frame.
- Cap active sends, spawn rate, projectile/effect counts, and automated resource output so neither long matches nor repeated inputs grow work without limit.
- Schedule harvest and cooldown work from server time. Avoid allocations in repeated targeting, movement, and message handling.
- Maintain an opt-in debug overlay with connections, latency, active counts, and economy totals; disable verbose diagnostics in release builds.

## Verification at each milestone

- Project compiles and the affected feature works in both player roles.
- Server and clients agree on health, ownership, currency, inventory, and outcome.
- Invalid, late, or repeated commands cannot grant resources or apply damage twice.
- Gameplay stays usable with measured network delay and at the milestone's target load.
- Death, disconnect, match end, and rematch release subscriptions, targets, pooled objects, and pending interactions.
- Ghost permissions hold on both clients and the server, including requests arriving after death. Respawn cannot occur twice or after match end, and death does not accidentally erase match economy or disable management.
- Item swaps/trades cannot duplicate gear or bypass class slots, energy, cooldowns, or ghost restrictions. All members of a technology group get the same catalog; other groups cannot build its towers without an explicitly designed exception.
- Match reset does not erase persistent unlocks or carry over temporary growth.
- Profile the added workload, address regressions, and commit a focused, working increment.

## Next action

The first standalone network slice and live Relay host/join/reset/rehosting are verified. Internet hosting/joining lives separately on `codex/internet-join-codes`, keeping the prior main-branch prototype available. Playtest its elevated hero-centered camera with WASD/right-drag orbit and share a Relay room code for a two-machine internet test. Next choose initial aim/attack bindings, sending costs/cooldowns, XP/gold reward rules, energy costs, tower attacks, castle damage, and ghost timer/respawn defaults for Milestone 2. Decide detailed economy, invasion, loot/crafting, and unlock rules before their respective milestones.
