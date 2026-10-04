# Tower Defense PVP - Development Plan

This is the living implementation sequence. See [GAME_DESIGN.md](GAME_DESIGN.md) for the gameplay specification, assumptions, and unresolved rules. Update both documents when a design decision changes.

[DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) records actual additions, verification, and release history. Update it with every meaningful implementation/build/release increment; a plan entry does not mean a feature is done.

## Current state

- **Owner release policy, 2026-10-03:** publish verified playable changes as versioned Windows/Mac experimental GitHub Releases for testing. **[0.2.1 is published](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.2.1)** at fixed `a5ae00e`; both complete anonymous ZIP downloads passed size/SHA256 checks, and older releases/stable v0.1 remain unchanged. Use the normal tested source-branch push/publisher without merging main mid-milestone, replacing old assets/tags, claiming full milestone completion or enabling paid services. Publication/download verification remains a separate gate from compilation/local testing; document failures honestly. Documentation-only edits and incomplete experiments need no new binary release.

- **Latest playable increment: 0.2.1**, `codex/milestone-2-combat`, based on pushed main checkpoint `a008f71`. First combat slice implemented and published experimentally: sends, XP upgrade, kill gold, group starter towers, weapon/energy use, castle results, ghost/home respawn and owner-private economy. Final 110/110 EditMode tests and Windows two-process combat/controls/reset/rejoin/rejection regression passed; ghost/results used explicit development fixtures. Windows/Mac packages are downloadable; Mac static checks passed, not Mac execution. No Milestone 2 completion or main merge is claimed. Next: real-peer combat/balance and remaining latency/platform checks; subsequent playable changes use 0.2.2.

- **Owner-reported peer play / Milestone 2 start, 2026-10-03:** Windows Steam and EOS friend play and Mac/Windows Steam play passed per owner feedback; EOS Mac/Windows is still unverified and is not assumed to pass. Exact test versions, both-host-role/reset/invite cases and latency/loss measurements were not supplied. Start private Milestone 2 (target 0.2) with editable provisional rules while keeping these remaining checks open. The owner authorizes a completed-branch main merge/push at each milestone transition, superseding the previous source-push pause; current public source visibility and fixed releases/tags are unchanged.

- **Local fog-of-war milestone update**, `codex/multiplayer-flow-design`: Milestone 4 now delivers fog of war before invasion. Hidden enemy locations/stats are withheld from ordinary clients, not merely obscured on screen; ghosts grant no vision. Milestone 2 establishes life-state/visibility permissions and Milestones 2-3 separate private state from opponent-visible events. Prediction, compact action updates/audits and automatic replacement-host recovery remain planned networking work, not shipped features. Recovery checkpoints must not silently expose hidden state to all players; player-host access to full authority state remains an explicit limitation. No runtime/build/version change or GitHub push in this documentation increment.

- **Local pre-Milestone-2 design update**, `codex/multiplayer-flow-design`: [MULTIPLAYER_FLOW.md](MULTIPLAYER_FLOW.md) specifies solo/premade queues for 1v1, 2v2v2v2 and 4v4; class/build locked before queue; whole-party team assignment; all-ready/loading gates; reconnect identity/state; one-minute 1v1 forfeit; and distinct full-premade versus mixed/random-filled leaving policy. These are design requirements, not enabled features. Team grace, buff/penalty values, authority survival and team gameplay rules remain decisions/gates. No Milestone 2 implementation begins here. Owner chose to keep GitHub public but stop further source pushes; subsequent design/log changes stay local.

- **0.1.5 EOS guest networking**, `codex/eos-guest-networking`: selected backend is connected through anonymous Connect Device ID, code-selected EOS lobbies and a custom bounded NGO P2P/relay transport. 92 tests and real single-device login/canceled-create cleanup/NGO host/leave/fresh-rehost passed; final local packages/regressions are recorded in the log. Still no real two-peer/home guest reachability or Mac execution proof. Future queue requests, immutable same-pool party rosters and atomic same-team reservations support planned four teams of two, but the active mode remains 1v1 and the queue/party service/UI/eight-player map are not implemented. See EOS_SETUP.md. No main merge, trusted server, cloud progression or paid billing is implied.

- **0.1.5 published experimental release:** [Windows/Mac downloads](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5), fixed source `855f4af`, both complete anonymous downloads verified. Exact-build publisher LAN and Mac static checks passed. Main/older tags/assets/latest stable v0.1 remain unchanged. Same-Windows-account guest self-connect reports a generic transport failure/incorrect destroy-cleanup warning; that known issue is disclosed, not fixed by publication.

- New source increment **0.1.4**, `codex/unified-sign-in`: one startup Steam check, automatic menu entry after a valid SDK sign-in, explicit guest choice/retry if unavailable, and a versioned device-local guest identity. 75/75 tests, both local packages, Windows guest/automatic-Steam/LAN checks and Mac static validation passed. Owner confirmed separate Steam and guest matchmaking; EOS is newly selected for future guest internet integration, superseding the old cancellation. Portal setup exists, but EOS SDK/credentials/rooms/transport are not enabled. Guest online actions stay disabled; LAN remains. This increment does not complete Step 2, cloud saves, EOS peer verification, Mac runtime, or a new release/main merge. Published 0.1.3 stays fixed.

- Unity project: `Tower Defense PVP`, Unity `6000.6.3f1`, Universal Render Pipeline.
- Repository contains the Unity project with generated caches, logs, and builds ignored.
- Historical Step 1 release is `0.1`; new owner policy is `0.<milestone>.<progress>`. Start Milestone 2 at `0.2.1`, increment to `0.2.2`, and start Milestone 3 at `0.3.1`. Do not rename/move historical tags. Use the versioned build commands and keep only the latest successful local folder/ZIP per platform. The owner removed the root shortcut; do not recreate it. Publish complete downloadable ZIPs as GitHub Releases, not tracked binaries in main.
- Current compatibility update: owner-authorized Windows/Mac 0.1.3 on `codex/macos-playtest`, targeting Universal Intel + Apple Silicon with Mono/macOS 12+, is published experimentally at fixed `v0.1.3` / `c70dd13`. 55 tests, both builds, Windows LAN/single-host Steam, Mac static packaging and both anonymous full-download checks passed. Mac static checks and a real friend/Mac runtime test are separate gates; no Developer ID signing/notarization is configured. Main remains 0.1.2 until another merge is authorized. See [MAC_TESTING.md](MAC_TESTING.md) and the development log.
- Recovery policy: one feature branch per update, focused commits, fixed `v<version>` release tags, and retained downloadable builds. Never move published tags, replace existing releases, or force-push/delete prior history. Recover by downloading an old ZIP or creating a new branch from an old tag; main is changed only by an explicitly approved merge.
- Latest cleanup decision: catch main up to current 0.1.2 source, then delete the other local/remote branches once their history is contained in main. Keep old release tags/downloads, not old local Builds copies. Real two-account/device Steam verification remains pending despite this explicitly authorized source merge.
- Latest publishing decision: the owner authorized making the 0.1.2 App ID 480 development-test ZIP publicly downloadable as a clearly experimental GitHub prerelease. Publication work is on `codex/public-playtest-download`; the playable mode/provider boundary comes from `codex/private-steam-playtest`. 47 automated tests, Windows packaging/launcher, native private host/leave/rehost and LAN regression passed. Guest internet/login/save functionality is not implemented. EOS remains canceled; no guest backend/billing is selected. Two-account/device Steam P2P verification remains pending; own-game App ID is needed for production distribution, not SDK testing. Stable 0.1's fixed tag/download remain intact, and public ZIP availability does not change friends-only lobbies.
- Foundation and the first network-smoke implementation are now present under `Assets/TowerDefense`: menu/class selection, ready lobby, two straight lanes, 3D blockout heroes, server-authoritative movement, hero-centered orbit cameras, starter hotbar, and management-panel placeholders. See README.md for play/build instructions and the implemented-versus-planned boundary.
- The original generic wave-defense plan has been revised around classes, player-sent units, three currency roles, land, materials, and invasions.
- Confirmed presentation/controls: 3D models, continuous hero-centered camera follow, and WASD hero movement. Start from an elevated/top-down angle and interpret right-click dragging as orbiting around the hero; camera tuning can change during development.
- Confirmed death rule: timed respawn with a controllable ghost that can roam and buy/sell/interact with its own side, but cannot attack, collect resources, or reveal fog of war. Ghost movement/camera position never creates vision.
- Future-system requirements: scrollable weapon/item hotbar, shortcuts for match management panels, configurable class energy pools, and equipment slots with class-specific starter gear. Loot/crafting/upgrading rules remain undecided.
- Future online modes: solo/friend/team queues for **1v1, 2v2v2v2 and 4v4**. Every player selects class/build before queue and readies individually in a complete staging lobby; loading acknowledgements precede play. Keep parties independent of match lobbies, authenticated identities independent of transport IDs and whole parties on one team. Preserve separate Steam/Steam-test/guest pools. Owner confirmed no buff/extra leaver penalty for teams fully premade before queue; mixed teams receive a reconnect opportunity followed by abandonment removal and a modest compensation buff. 1v1 has an exact 60-second rejoin/forfeit rule. See MULTIPLAYER_FLOW.md for unresolved timers/penalties, anti-abuse tests, authority survival and map/performance gates. Existing 0.1.5 contracts do not implement these services or 4v4.
- Roster target: five technology groups with five classes each (25 total). Warrior and future bow-wielding Hunter are Primate; Wizard is Mystic. Classes in the same group share its tower catalog. Three group names and the remaining roster are not yet supplied.

## Target and sequence

2026-10-03 operational checkpoint: owner says EOS works and requests continued development. Treat connectivity as operational for private work without turning that into an independently verified Steam/full-platform pass. Completed Milestone 2 source was checkpointed/pushed on main at `9e6abcd`; start **0.3.1** on `codex/milestone-3-economy`. Economy defaults are provisional assets pending the owner's detailed design. Validate transactions, both-role replication, ghost permissions/reset and paired packages, then publish a fixed experimental release. Detailed combat/balance/platform/latency/load checks remain open alongside development.

Build one small 1v1 map with two straight lanes. Prove two-client networking early, then add combat, economy, harvesting, and invasion in small playable milestones.

Brief local experiments are useful, but do not build the entire game locally before introducing multiplayer. Each milestone must work with two players before the next dependent milestone begins.

## First implementation defaults

- Target: Windows and experimental Universal macOS prototype. Unity Netcode for GameObjects 2.13.3; one player hosts. Unity Transport supports LAN diagnostics; Steam uses Steamworks.NET/P2P/lobbies and 0.1.5 adds independent EOS guest Connect/rooms/relay transport. The older published 0.1 uses Unity Relay. Current scenes load locally before connection; no queue, persistent rewards, reconnect/host migration or dedicated deployment yet. Real guest/Steam two-device and Mac runtime gates remain open. Future staging/loading/recovery must follow MULTIPLAYER_FLOW.md rather than treating today's two-client flow as eight-player-ready.
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
| 1. Network smoke test (0.1) | Menu, two classes, 3D arena, WASD, local cameras, starter inventory/hotbar and shortcuts | Player/selection state agrees; cameras and UI input are independent |
| 2. Sending and defense | Sent units, one tower per initial group, class attacks/energy, XP/gold, ghost respawn | Combat, energy, group tower access, and respawn agree on both clients |
| 3. Land and economy | Buy land, harvest, build/trade, hero leveling, inventory and ghost permissions | Transactions cannot duplicate items/resources or overspend; ghosts cannot collect |
| 4. Fog of war and invasion | Visibility-filtered enemy state, no ghost scouting, then crossing/fighting/theft/return | Hidden enemy updates do not reach ordinary clients; ghosts grant no vision; both players can invade and recover |
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

## Multiplayer delivery gates (planned, not all required before private combat work)

1. **Before Milestone 2 starts:** finish documenting the owner-confirmed flow in GAME_DESIGN.md/MULTIPLAYER_FLOW.md and preserve stable match-player/profile/team/seat/build identities. Owner-reported Windows Steam/EOS and Mac/Windows Steam play now allow private combat development; EOS Mac/Windows and detailed peer regression/latency checks remain open. Do not infer them from the owner's confidence that they will pass. No public queue, complete platform matrix or reconnect claim is made.
2. **During the 1v1 combat/economy increment:** separate disconnection from hero death and match-state ownership. Support idempotent commands/results and recovery-ready snapshots so reconnect can bind a new transport ID without recreating resources, towers, inventory or respawn timers. Keep private inventory/economy/authority state separate from opponent-visible events; a future recovery snapshot is not permission to broadcast everyone's hidden state. Establish that Ghost is not a vision source before implementing fog in Milestone 4.
3. **Before public 1v1 queue play:** implement party consent/membership, immutable class/build tickets, provider/version/mode filtering, cancel/timeout/assignment cleanup, fair whole-party packing/authority selection, full-roster individual readiness and synchronized loading. Keep private room codes available for friend challenges.
4. **Before reconnect/forfeit guarantees:** implement/test the owner's preferred automatic replacement-host recovery only after its state-recovery and fog-privacy design is resolved within the no-usage-billing constraint. Elect one suitable remaining host, restore checkpoints/accepted actions, authenticate reserved seats, reject stale authority messages, preserve original deadlines and finalize results once. A full readable backup on every client conflicts with hidden enemy data; ordinary observer filtering cannot hide full simulation state from a player who becomes host. Test host crash/quit as well as client loss, reconnect just before/after 60 seconds, duplicate connections and both players abandoning. Current player-hosted relay cannot preserve a match after its host exits; no trusted-host or seamless-migration guarantee is implied.
5. **Before 2v2v2v2/4v4 queues:** add actual format/map/team/castle/lane/ally/economy/send/victory rules, eight-player admission/performance budgets, frozen premade classification and selected team grace/buff/leaver policy. A full party gets no buff or extra leaving penalty; a mixed team gets compensation only after abandonment. Test partial/full parties, whole-team loss, late rejoin, intentional abandonment and modifier stacking.

The 1v1 combat prototype starts at 0.2.1. Team queues, compensation and reconnect runtime work are separately authorized increments, not silently included in this combat slice.

## Milestone 2 - Sending, defense, and castle victory

1. Define sending cost/cooldown, XP award timing, kill reward owner, unit upgrade scope, and the first two classes' energy/cost/recovery rules. Choose one starter tower for each initial group.
2. Implement one sendable unit type. An accepted server command creates units on the opponent's lane and awards XP according to the selected rule.
3. Implement authoritative path movement, health, damage, death, and arrival at a castle.
4. Add one basic attack per hero using the selected weapon and generic energy/cooldown rules. Add one starter tower each for Primate and Mystic using shared targeting/fire code. Start each side with its group's preset tower; purchasing comes next.
5. Reward defender gold exactly once per eligible kill. Keep match XP and gold as separate balances.
6. Implement one XP-funded sending-unit upgrade. Connect the upgrade and troop-sending shortcuts to their panels; show costs/effects in the HUD.
7. End the match on castle destruction under the agreed damage rule and support a clean reset.
8. Add authoritative hero death, controllable ghost movement, and timed respawn. Choose an initial timer/location/health policy before implementation. Keep the camera on the controlled hero/ghost and prevent ghost attack requests. Establish life-state vision eligibility: ghosts grant no vision; remove the living hero's vision contribution on death and restore it only when alive. Full fog rendering/observer filtering follows in Milestone 4.
9. Validate item ownership, selected/equipped weapon, life state, cooldown, and all energy costs together before accepting hero actions. Test an empty pool, rapid item changes, and simultaneous energy spending; inventory selection must not bypass ghost restrictions.

Gate: two players send, defend, upgrade, use valid class equipment/energy, die/roam/respawn, and finish a match with identical authoritative results. Validate group tower access, rejected sends, duplicate commands, simultaneous kills/energy costs, ghost attack rejection, respawn deadlines, and match-end command rejection. The prototype must not leave a player without control during the death timer.

## Milestone 3 - Land, harvesting, and hero growth

First 0.3.1 slice: two assigned plots/nodes per side, one stone material, group-indexed tower build menu, partial own-asset refunds, and one match-only maximum-HP purchase with no instant heal. Price/yield/capacity/range/time/recovery effects are editable in EconomyRules; see README. This is not persistent unlock power, a full inventory/crafting system, or completed measured acceptance. Timed collection resolves after damage and match-end checks; ghost management uses the same atomic costs/ownership checks as living management.

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

## Milestone 4 - Fog of war and invasion

Deliver fog first, then invasion on top of it. This is planned scope, not implemented in 0.1.5. Keep the existing milestone/version sequence; no extra release number is assigned by this design edit.

1. Choose vision radius/occlusion, eligible living revealers, own-territory intruder detection, team sharing, and last-seen behavior. Define which combat stats are shown while visible; private economy/inventory must not automatically become public. Do not assume entering enemy land reveals the entire enemy side. See GAME_DESIGN.md's fog specification; territory-wide automatic detection remains undecided.
2. Implement authority-controlled visibility and client fog presentation. Withhold hidden enemy hero locations/stats and other visibility-protected entities from ordinary clients. Stop live updates when visibility ends; send a fresh permitted snapshot on reveal. A remembered position/stat is stale, never a live tracking feed. Separate rendering visibility from network disclosure and from combat targetability.
3. Enforce **no ghost scouting**: ghost movement, orbit camera, UI and any ghost-only interaction create no reveal area or new enemy intel. Remove the dead hero's contribution immediately without erasing independent authorized vision from other living/allied sources. Respawn restores only the living hero's allowed vision. Own-side management remains available.
4. Filter every disclosure path, including object spawns/transforms, stats, targeted RPCs, action-ledger entries, effects/audio, UI/minimap and reconnect snapshots. Validate target/action permissions on the authority; hiding an object is not by itself a rule that all attacks must fail. Resolve replacement-host checkpoint privacy before enabling migration with fog; disclose player-host full-state access rather than claiming cheat-proof secrecy.
5. Pass the visibility gate below before implementing invasion access, attackable targets, theft source/type/limits, return rules, carried-resource handling and ghost roaming limits. Then implement crossing/the agreed entrance, selected hero-combat targets and one atomic resource-theft interaction.
6. Integrate invasion with death/ghost/respawn. Ghosts cannot attack, steal or reveal fog. Release combat targets and harvesting actions on death; preserve valid own-side management while keeping disconnect separate from death. Test home-lane pressure and recovery after a failed invasion.

Visibility gate: test both roles entering/leaving vision, camera orbit, death while revealing, ghost travel into hidden territory, respawn, UI/tooltips, stale/out-of-order updates, reconnect and rematch. Inspect ordinary-client payloads/state, not only screenshots: hidden live enemy positions/stats must be absent from all delivery paths. Measure visibility-transition correctness and bandwidth/CPU with simulated delay/loss and representative entity counts; spatial filtering must not flicker or leak updates. Host full-state access is a documented trust limitation, not a passing secrecy test. Vision radii, transition policy and performance budgets are chosen/measured in this milestone.

Invasion gate: invading is useful but carries an opportunity cost. No resource duplication, ghost theft/damage/scouting, wrong-side building, repeated death rewards, hidden-state tracking, or permanent loss of player control. Both clients agree when the ghost timer ends and the living hero returns.

## Milestone 5 - First complete playable prototype

1. Connect Main Menu, Store placeholder, class selection, lobby, match, results, and rematch.
2. Add a readable HUD for castle health, hero/ghost state, respawn countdown, class energy pools, hotbar/equipped items, gold, XP, materials, and sending/building actions. Finish essential panel shortcuts without input conflicts.
3. Finish essential prompts and match reset. Implement the applicable MULTIPLAYER_FLOW.md disconnect/reconnect/forfeit rules only once their authority-survival/state-recovery gate is passed; clearly label any temporary private-test disconnect behavior. Do not advertise a reconnect policy the current host architecture cannot support.
4. Optionally add one authored neutral challenge once the required economy and invasion features are stable.
5. Measure a standalone development build under normal and stress conditions. Establish provisional caps for units, towers, effects, and resource production from the results.
6. Test delay, packet loss where supported, simultaneous actions, repeated matches, and both host/client roles.
7. Save a focused commit and a reproducible internal build with play/test instructions.

Definition of done: two players can select Warrior/Wizard with Primate/Mystic tower access, use starter gear through a scrollable hotbar and the chosen energy rules, open management panels by keybind, send/upgrade units, grow their economy/hero, build on bought land, invade under visibility-filtered fog of war, die into a controllable ghost, manage their own side during a synchronized timer, respawn, finish, and rematch. Ghosts cannot attack, collect resources or reveal fog. Ordinary clients receive only permitted enemy intel; player-host secrecy limits remain explicit. Fresh matches reset temporary growth, inventory/equipment acquisitions, and energy state to the selected starter setup. The complete Store/unlock system and loot/crafting content are not required for this prototype.

## Milestone 6 - Expansion order

1. Add class-unlock progression, starter equipment alternatives, and a small Store using a versioned local profile for private tests. Decide currency/prerequisite/mastery requirements, show difficulty/group labels, and keep beginner classes viable against later unlocks.
2. Add Hunter with a bow as an early extension test: new class/weapon behavior, same Primate tower catalog as Warrior. Then introduce one class from a newly named technology group to prove that adding a group/catalog does not require rewriting UI or building rules.
3. Add one simple found-gear interaction after choosing loot ownership/death handling. Decide slot compatibility and equipment effects, then add crafting/upgrades only once acquisition rules, recipes, and ghost permissions are settled. Validate inventory, hotbar, and equipment ownership across each transition.
4. Expand toward all five groups and approximately 25 classes, using supplied class designs. Add class-specific abilities/energies/equipment, group tower sets, sent units, and upgrades through existing definitions. Verify whole-loadout/group balance and performance in small batches.
5. Add authored maps and challenges with the existing path/plot/node schema. Test invasion routes and resource access for fairness.
6. Add automatic harvesting as a match investment using the existing harvest transactions. Bound output and schedule work without an update loop per node.
7. Steam/EOS guest networking was brought forward before Step 2; complete real peer/Mac checks with the matching 0.1.5 downloads. Keep App ID 480 explicit and use the game's own identity before production Steam distribution. Add queues, persistent parties, authenticated reconnect and team modes through the delivery gates above/MULTIPLAYER_FLOW.md; guest and Steam pools remain separate. Add versioned durable saves before permanent unlocks. Neither relay is a trusted match-result server; authority survival and costs need a deliberate choice, not paid billing enabled by default.
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
| Party and queue flow | Immutable pre-queue build/party/mode contracts, pool/version gates and whole-team placement | Consent/invites/leadership, coordinated tickets, fair matching, cancellation and all-ready/loading UX |
| Reconnect and leaving | Stable match-player identity, state ownership, snapshots and idempotent results | Surviving authority, authenticated reserved seats, deadlines, 1v1 forfeit and mixed-team compensation |
| Fog of war | Life-state vision eligibility in Milestone 2; separate private/public state in Milestones 2-3 | Milestone 4 visibility-filtered enemy state and fog presentation before invasion; no ghost scouting; resolve recovery/host trust limits |
| Team modes | Data-driven teams/seats and class-to-group catalog ownership | Actual 2v2v2v2/4v4 formats/maps/rules, frozen party-origin policy and eight-player measured budgets |

## Architecture rules that protect later work

- Keep rules separate from networking, rendering, UI, and audio. The same rules should run in local tests and on the match authority.
- Keep the camera controller and input-to-world mapping separate from hero simulation. Use 3D world positions for movement, aim, and placement; changing camera framing must not change authoritative gameplay or reveal hidden state.
- Use data assets for reusable definitions; never store a player's changing balances, health, or cooldowns in shared definition assets.
- Resolve towers from class -> technology group -> shared catalog. Keep class-specific equipment/abilities/energies separate; do not hard-code a 25-class limit, duplicate catalogs per class, or trust a client-submitted group/tower choice.
- Give inventory items/instances, equipment slots, abilities, energy types, and input actions stable IDs. A hotbar or equipped slot references owned state; it must not copy/duplicate items. Recompute equipment-derived values from the equipped set so repeated equips cannot stack modifiers accidentally.
- Use generic per-class energy pools and data-driven costs/recovery; validate all costs and cooldowns before applying an action. Keep energy distinct from XP/gold/materials and settle death/max-capacity behavior before adding energy-changing gear.
- Route input through named actions and UI/gameplay contexts. Reserve wheel selection and right-drag orbit, prevent menu input from also triggering gameplay, and keep local UI selection separate from authoritative equip/use transactions.
- Every networked entity and player has stable identity and explicit ownership. Validate actions by identity, range, state, and permissions on the server.
- The authority decides vision and permitted enemy disclosures independently of camera position. Ghosts never supply vision. Filter object state, event/ledger payloads and reconnect views consistently; a complete authoritative recovery snapshot must not be broadcast to ordinary opponents. Player-host full-state access is not fixed by observer filtering.
- Keep profile/account identity separate from per-session Netcode IDs and from display names. Provider namespaces distinguish Steam, private-test and guest profiles. Room/transport adapters must not put Steam dependencies into combat/economy rules; remote account claims still require provider/server validation when persistence is added.
- Freeze pre-queue build/party origin and assigned team/seat independently of social-party changes. Reconnect restores existing match ownership to an authenticated new connection; it must not create a second starting inventory, reset life state or prolong its own deadline. A player-hosted process dying is authority loss, not merely a missing client.
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
- Address the owner's non-host responsiveness concern in a separately authorized networking increment: measure delay/loss, then implement local hero prediction with authoritative input validation/reconciliation and remote interpolation as appropriate. Follow the predicted local hero with the camera; keep physics/state corrections separate from visual smoothing. Tick/update rates and correction tolerances require measurement, not a promise that fewer packets remove latency.
- Replicate permitted gameplay state at appropriate rates; create cosmetic audio and effects locally from permitted events. Visibility filtering is required for fog even at low entity counts, not just a later load optimization. Send compact input batches/changed state and use reliable deduplicated transactions; periodic consistency audits supplement immediate rule validation, never replace it. Keep logs/audits subject to the same enemy disclosure rules.
- Share catalog/definition IDs and update mutable equipment, selection, and energy state as needed. Keep private inventory details visible only to authorized players; send opponents the equipped appearance/gameplay information they need. Avoid sending full catalogs or rebuilding hotbar/equipment UI every rendered frame.
- Cap active sends, spawn rate, projectile/effect counts, and automated resource output so neither long matches nor repeated inputs grow work without limit.
- Schedule harvest and cooldown work from server time. Avoid allocations in repeated targeting, movement, and message handling.
- Maintain an opt-in debug overlay with connections, latency, active counts, and economy totals; disable verbose diagnostics in release builds.

## Verification at each milestone

- Project compiles and the affected feature works in both player roles.
- The authority maintains consistent health, ownership, currency, inventory and outcome; each client agrees with its permitted view. Fog/private-state filtering must not be bypassed just to make every client hold identical hidden enemy data.
- Invalid, late, or repeated commands cannot grant resources or apply damage twice.
- Gameplay stays usable with measured network delay and at the milestone's target load.
- Death, disconnect, match end, and rematch release subscriptions, targets, pooled objects, and pending interactions.
- Ghost permissions hold on both clients and the server, including requests arriving after death. Respawn cannot occur twice or after match end, and death does not accidentally erase match economy or disable management.
- For Milestone 4 onward: hidden enemy positions/stats are absent from ordinary-client deliveries, ghosts grant no vision, camera/UI/ledger/reconnect do not bypass fog, and respawn/rematch correctly rebuild eligible vision. Do not report ordinary-client filtering as protection against a modified player host.
- Item swaps/trades cannot duplicate gear or bypass class slots, energy, cooldowns, or ghost restrictions. All members of a technology group get the same catalog; other groups cannot build its towers without an explicitly designed exception.
- Match reset does not erase persistent unlocks or carry over temporary growth.
- Profile the added workload, address regressions, and commit a focused, working increment.
- For queue/reconnect/team increments: test complete-roster readiness/loading, compatible pools, whole-party cancellation/admission, duplicate tickets, host/client loss, expiry/result races, premade/mixed classification and compensation exploits. Do not infer eight-player readiness from today's two-player tests.

## Next action

Current 2026-10-03 instruction supersedes the older progression blocker below: owner accepts networking as operational and requested the next step. Main now holds completed Milestone 2 at `9e6abcd`; develop/validate/release 0.3.1 economy on its separate branch. Keep all old tags/assets fixed. Do not label the assumption about Steam as a new observed test or imply a complete milestone/balance/performance pass. Match-only provisional rules allow further design changes without persistent save migration.

2026-10-03 superseding update: the completed Milestone 1/design work was merged and pushed to main at `a008f71`. Local Milestone 2 work starts at **0.2.1** on `codex/milestone-2-combat`, with editable combat defaults documented in README. Version policy is `0.<milestone>.<progress>`; starting a milestone does not imply its completion gate passed. Increment within a milestone; start Milestone 3 at 0.3.1. Main pushes occur at owner-authorized completed-milestone transitions. Keep published tags/downloads fixed. Verify combat, ghost/respawn, private replication, results/rematch and real peer play before moving on; EOS Mac/Windows remains unverified. No prediction/fog/migration/queues or paid service is silently included.

### Earlier checkpoint context (superseded where noted above)

Published experimental [v0.1.5](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5) has matching Windows/Mac EOS guest and explicit Steam-test downloads at fixed `855f4af`; both anonymous downloads passed size/SHA256 checks. Older tags/ZIPs/latest stable v0.1 and main remain unchanged. Keep only the latest successful local package per platform, preserve recovery history and leave the root shortcut absent. The owner chose to keep the repository public while stopping further source pushes; current design/log edits remain local on `codex/multiplayer-flow-design`. No source deletion/visibility change/new repository or main merge is authorized.

Next: verify the documented multiplayer design, then friend-test **matching 0.1.5** on separate devices/home networks, both guest host roles, room/ready/reset/leave/rejoin/member rejection and replicated controls. Same-Windows-account copies must use LAN / THIS PC; generic self-connect failure/role-aware cleanup is a separately authorized follow-up. Test actual Mac launch/security/render/input and native Steam/EOS loading; independently test Steam with separate accounts using explicit App ID 480. Do not require purchasing an App ID for development tests. Queue/party/reconnect/team-mode service work and persistent saves are not implemented by these checks.

For Milestone 2, choose initial aim/attack bindings, sending costs/cooldowns, XP/gold reward rules, energy costs, tower attacks, castle damage, and ghost timer/respawn defaults. Decide detailed economy, invasion, loot/crafting, and unlock rules before their respective milestones.
