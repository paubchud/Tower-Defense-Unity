# Tower Defense PVP

A 3D, hero-controlled 1v1 tower-defense prototype. The first implementation is the multiplayer foundation, not the complete combat/economy game.

## Open and play

1. In Unity Hub, open **`Tower Defense PVP`**, the project folder inside this repository. Use Unity **6000.6.3f1**.
2. Open `Assets/TowerDefense/Scenes/MainMenu.unity` and enter Play mode.
3. Select **Play**, choose **Warrior** (Primate) or **Wizard** (Mystic), then **Host Match** or **Join Match**.
4. For two copies on this computer, join `127.0.0.1`, port `7777`. Run the Windows build alongside the editor, or run two build instances. Both players must click **Ready**.
5. For a private LAN test, the joining player enters the host computer's LAN IPv4 address. Both use the same port; allow the game through the private-network firewall if prompted. Internet matchmaking/relay and two-machine testing are not provided by this increment.

A local development build is created at `Builds/Prototype/TowerDefense.exe`. The generated build and validation logs/screenshots are intentionally ignored by Git.

## Controls

| Control | Action |
| --- | --- |
| WASD | Move relative to the camera |
| Hold right mouse + drag | Orbit/tilt around the hero, always keeping the hero centered |
| Mouse wheel / click a hotbar slot | Select weapon, tool, or empty slot |
| B | Match shop placeholder |
| U | Upgrade placeholder |
| T | Troop-sending placeholder |
| I | Starter equipment and class information |
| Esc | Open/close pause menu; multiplayer itself does not pause |
| Menu button | Leave the session and return to class selection via the main menu |

The host's pause menu includes **Reset to Lobby**. Both players must ready again. Opening a management panel blocks local movement/orbit; scrolling over UI does not also change held items.

## Implemented in this increment

- Main Menu, Store explanation, data-driven Warrior/Wizard selection, host/join setup, two-player ready lobby.
- Blockout 3D heroes with distinct starter weapon/clothing/tool visuals and a synchronized three-slot starter hotbar.
- A small authored-map definition with two straight lane paths, two castles, land markers, resource markers, and open space between sides. Markers do not yet transact or harvest.
- Server-authoritative movement, class/side assignment, readiness, active hotbar slot, and lobby reset. Only the owning player can submit their movement/selection requests. The host rejects a third connection and resets to the lobby when the opponent leaves.
- Independent local hero-follow/orbit cameras with scenery obstruction handling.
- Class -> technology group -> shared tower catalog references, generic energy definitions/pool logic, and per-player starter inventory containers. These are foundations; tower firing, energy-using attacks, mutable loot, and crafting are not implemented yet.

Store stock, attacks, sent troops, tower construction, currencies, harvesting, ghost/respawn, theft, progression, and victory/results remain future milestones. Walking across the blockout is a connectivity test, not a finalized invasion rule.

## Content and code

Content assets are under `Assets/TowerDefense/Content`. `PrototypeCatalog` references classes and `StraightDuel`; each class references its technology group, starter items, and energy definitions. Adding a class does not require a new network message or a duplicate tower list. The current selection screen is laid out for the initial two classes; a paged/scrollable roster is needed before expanding to 25.

Warrior currently has a sword, mail, pickaxe, and a provisional 100-capacity stamina definition. Wizard has a staff, robes, pickaxe, and a provisional 100-capacity mana definition. These editable defaults are not final balance decisions. No gameplay energy spending is connected yet.

Map lane endpoints/spawns, plot positions, and resource positions are editable in `StraightDuel`. Runtime geometry currently builds a simple blockout from those positions; it is not the final map-art workflow.

Runtime folders separate Data, Core rules, Input, Networking, Presentation, UI, and development-only Diagnostics. Editor generation/build tooling and NUnit tests have separate assemblies.

Networking uses Unity [Netcode for GameObjects 2.13.3](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/changelog/CHANGELOG.html) and Unity Transport. A player-hosted private prototype is authoritative over its clients, but is **not a trusted competitive/reward server**. Movement currently uses server simulation and transform interpolation, not client prediction; latency testing and performance budgets remain later work.

## Build and validate

Assets/scenes and their `.meta` files are checked in. **Tools > Tower Defense > Create Missing Prototype Assets** can create missing foundation assets; it deliberately does not overwrite existing authored assets. Open MainMenu afterward.

Use **Window > General > Test Runner > EditMode** for the foundation tests. To create the Windows development build, run this in PowerShell, adjusting the Unity installation path if necessary:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath "$PWD\Tower Defense PVP" -executeMethod TowerDefense.Editor.PrototypeBuilder.BuildWindows -logFile "$PWD\Tower Defense PVP\Logs\prototype-build.log"
```

Close the editor before a command-line Unity run against the same project. The executable can return control to PowerShell before the build is finished; confirm `TD_BUILD_PASS` in the log.

The development build has opt-in command-line smoke diagnostics: `-td-smoke-host` and `-td-smoke-client` use localhost port **7779**, automatically ready up, move, select items, and check replicated state/camera centering. Virtual input devices exercise the real WASD, orbit, wheel, and five panel bindings, including blocked gameplay while panels are open. They then reset, leave, and rejoin with swapped classes to check cleanup. These diagnostics are inactive during normal play and absent from release gameplay. A third instance with `-td-smoke-client -td-expect-reject`, launched while both players are connected, checks the two-player limit without resetting the accepted players. `-td-captures <existing-directory>` renders the live menu/arena/equipment UI to PNGs, including when hidden test windows skip screen presentation. Each process logs explicit `TD_*_PASS` or `TD_*_FAIL` results before exiting; consult each process's `-logFile` output.

After building, run `powershell -ExecutionPolicy Bypass -File .\Tools\ValidatePrototype.ps1 -Capture` from the repository root to launch all three test processes at the correct time and check their exit codes/pass markers. It saves each run in a separate ignored validation folder and only stops processes it started. This invocation bypasses policy for this one process; it does not change the machine's execution-policy setting.

Known rendering warning: the starter URP settings can report stripped, unused depth-of-field/Panini postprocessing shaders when first rendering a development build. The prototype does not use those effects; the lit 3D scene and UI render correctly. Revisit the postprocessing configuration when introducing visual effects rather than keeping unused shader variants solely to silence a warning.

## Design and next milestone

See [GAME_DESIGN.md](GAME_DESIGN.md) for the intended game and [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) for the implementation order. Next: player-sent units, basic hero/group-tower combat, separate match XP/gold, castle victory, and timed ghost respawn. Decide the initial combat, sending, reward, and respawn rules before wiring those transactions.
