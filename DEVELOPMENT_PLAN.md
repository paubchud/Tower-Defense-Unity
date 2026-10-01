# Tower Defense PVP — Development Plan

This document is the working sequence for building the game. It should be updated whenever a major system or design decision changes.

## Initial target

Build a small, playable 1v1 multiplayer test map with:

- One straight path per player.
- One spawn point and one base/goal per player.
- A minimal tower placement and targeting loop.
- A minimal enemy wave loop.
- Two connected players who can see the same match state.
- A clear win/loss condition and a way to restart the match.

The first map is intentionally simple. Its purpose is to prove the game loop, networking, scene structure, and performance assumptions before adding content or complicated map geometry.

## Recommended implementation sequence

### Phase 0 — Project and architecture foundation

1. Confirm the Unity version and render pipeline already stored in `Tower Defense PVP/ProjectSettings`.
2. Establish folders for scripts, scenes, prefabs, materials, UI, data, and tests.
3. Add a small bootstrap scene and a separate gameplay scene.
4. Decide the multiplayer authority model before gameplay depends on local-only state.
5. Create a basic version-control rhythm: one focused commit per milestone, with the project opening successfully after each milestone.

### Phase 1 — Offline gameplay prototype

1. Create the 1v1 test map with two mirrored straight lanes.
2. Add lane waypoints or a path representation; do not make enemies depend on scene object positions directly.
3. Add enemy spawning, movement, base damage, and cleanup.
4. Add one tower type with placement validation, range, targeting, fire rate, and damage.
5. Add a simple resource value used for tower placement and/or upgrades.
6. Add wave start, wave completion, defeat, victory, and restart states.
7. Test the complete loop locally before adding network synchronization.

### Phase 2 — Multiplayer foundation

1. Add the selected networking package and a small connection/lobby flow.
2. Use a server/host-authoritative match model for rules, resources, spawning, damage, and victory.
3. Treat clients as input/request sources; never trust client-submitted damage, currency, enemy health, or win state.
4. Network only the state that must be shared. Keep visual effects, UI animation, audio, and non-gameplay decoration local.
5. Synchronize player connection, ready state, match seed, wave state, towers, enemies, resources, and base health.
6. Test host plus one client on the same machine, then two machines, before adding more systems.

### Phase 3 — First multiplayer playable build

1. Support two players joining the same match.
2. Assign each player a lane and starting resources.
3. Allow both players to place the first tower type.
4. Run deterministic or server-controlled waves visible to both players.
5. Handle disconnect, reconnect policy, match end, and restart cleanly.
6. Add basic debug HUD information: connection state, tick/latency, wave, resources, and base health.
7. Make a tagged internal build and test it outside the Unity Editor.

### Phase 4 — Expand the game safely

Add features in this order, validating multiplayer and performance after each group:

1. More tower types and enemy types.
2. Tower upgrades and sell/refund rules.
3. Player-versus-player interaction or shared/competitive wave rules.
4. Map obstacles, branching paths, and map data assets.
5. Matchmaking, room codes, player names, ready screens, and rematch flow.
6. Audio, VFX, UI polish, tutorials, progression, and settings.
7. Persistence, accounts, analytics, ranked rules, and anti-cheat measures only after the core match is stable.

## Architecture rules

- Keep gameplay rules separate from presentation and UI.
- Keep network-facing commands separate from local visual effects.
- Make the server/host the source of truth for all competitive state.
- Store tower, enemy, wave, and map configuration in data assets rather than hard-coding values into scene scripts.
- Use stable IDs for players, towers, enemies, waves, and map objects where synchronization or save data may need them.
- Prefer reusable prefabs and components over large manager scripts.
- Keep the test map data-driven so later maps can use the same path, spawn, base, and placement systems.
- Avoid making individual enemies or towers perform expensive global searches every frame.
- Use object pooling for enemies, projectiles, damage numbers, and repeated effects once the basic loop works.
- Use fixed simulation steps for authoritative gameplay and avoid using frame rate as game logic.
- Keep deterministic behavior where practical; otherwise replicate authoritative results explicitly.
- Do not add persistence or progression until match state can be reset reliably.

## Performance guardrails

- Profile in a standalone development build, not only in the Editor.
- Establish a baseline on the first playable map: frame time, memory, network bandwidth, spawn count, and active object count.
- Avoid per-frame allocations in spawning, targeting, movement, and networking code.
- Use spatial filtering or controlled target lists instead of checking every enemy against every tower.
- Cap or batch visual effects so gameplay load cannot create unbounded particle or projectile counts.
- Keep network messages event/state based and small; do not synchronize transforms or cosmetic effects unnecessarily.
- Test at the expected maximum enemy and tower counts before adding polish.
- Add lightweight logging and debug overlays that can be disabled for release builds.

## Testing checklist for every milestone

- The project opens without compile errors.
- The scene starts, plays, ends, and restarts without stale state.
- A disconnected client cannot change authoritative game state.
- Host and client agree on wave, resources, health, and victory state.
- Repeated spawning and cleanup do not leak objects or memory.
- The game remains playable at the planned stress level.
- The change is committed with a short, focused commit message.

## First milestone definition of done

The first milestone is complete when two players can connect to a match, see the same two-lane test map, place one tower type, survive or lose to a simple wave, and restart without restarting the application. The match must run from a standalone build and the repository must remain free of Unity-generated folders.

## Decisions to make before Phase 2

- Which Unity networking solution and transport to use.
- Whether the first multiplayer test uses a host or a dedicated server.
- The target player count and expected platform.
- Whether the lanes are shared, competitive, or purely separate during the prototype.
- The initial tower, enemy, resource, and wave values.

