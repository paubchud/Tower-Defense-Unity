# Tower Defense PVP

A 3D, hero-controlled 1v1 tower-defense prototype. The first implementation is the multiplayer foundation, not the complete combat/economy game.

See [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) for completed changes, test results, release checkpoints, and unfinished work. It is updated with each meaningful increment.

Current source: **0.1.1 Steam preview** on `codex/steam-integration`. Steam code is implemented, but the game's own App ID and two-account/device multiplayer check are still required. The public GitHub download is the earlier **v0.1 Unity Relay prototype**, not the Steam preview.

## Open and play

1. In Unity Hub, open **`Tower Defense PVP`**, the project folder inside this repository. Use Unity **6000.6.3f1**.
2. Open `Assets/TowerDefense/Scenes/MainMenu.unity` and enter Play mode.
3. Select **Play**, choose **Warrior** (Primate) or **Wizard** (Mystic), then **LAN / This PC** while the Steam App ID is unset. Two copies on this computer use `127.0.0.1`, port `7777`; on a LAN the joining player uses the host PC's LAN IPv4 address. Both use the same port; allow the game through the private-network firewall if prompted. Zero-configured builds do not initialize Steam for LAN play.
4. After following [STEAM_SETUP.md](STEAM_SETUP.md), sign into Steam and choose **Host Steam**. Share **Copy Room Code** or use **Invite Steam Friend**. Your friend selects a class and uses **Join Steam** with that numeric code or the accepted invite. Two separate Steam accounts/devices with access to the game's App ID are required.
5. Both click **Ready**. The host must keep the game open. Share codes privately; these are friends-only, two-player Steam lobbies. Leaving the host session invalidates its code; hosting again creates a fresh one. No guest/EOS login or cross-store matchmaking is provided.

Double-click the repository's **TowerDefense.exe.lnk** shortcut to play the newest successfully packaged Windows build, currently the **0.1.1 Steam preview**. The builder automatically updates this same shortcut; old builds stay intact. The released Step 1 executable remains in `Builds/TowerDefense-0.1-Windows/TowerDefense.exe`. Builds and validation output are intentionally ignored by Git, while tested downloadable ZIPs are published as GitHub Releases.

## Send it to a friend

The currently published [v0.1 Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1) is the **legacy Unity Relay build**. Download its **Windows ZIP asset**, not GitHub's automatically generated source-code ZIP. Your friend extracts the whole ZIP and opens `TowerDefense.exe`. In that version only, use **Host Online / Join Online** with the room code, then both **Ready**. It requires neither Unity nor Steam, uses anonymous Unity sign-in, and has Relay service quotas. Both players must use the same Windows 64-bit build.

Do not distribute the Steam preview as Steam-ready while its App ID is zero. Once configured and verified, Steam testers need access to the game's App ID, a running signed-in Steam client, and the same new build; a GitHub ZIP alone does not grant Steam access. The active Steam branch has removed the Unity Services multiplayer package and Relay login/connector. No Unity cloud/Epic setup is needed for this branch. Local movement/lobby testing, not the complete combat/economy loop, is available.

## Steam-only publishing plan

The owner changed direction to **Steam-only compatibility and multiplayer**. EOS is canceled; no Epic developer setup is needed. Players will use their signed-in Steam account, Steam lobbies/invitations, and Steam P2P relay networking. Guest itch play and cross-store multiplayer are no longer requirements for this increment. No port forwarding and no metered third-party multiplayer-service charges remain requirements.

The 0.1.1 preview implements Steamworks.NET, lobbies/invitations, and a Netcode-compatible [Steam networking](https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay) transport, retaining host-authoritative gameplay/LAN diagnostics. Its native SDK/lobby check passed using Valve's private test app; real two-user P2P still needs the game's App ID and distribution/test access. **Steam integration is not in the published 0.1**; that build uses Unity Relay, which has a free allowance followed by [usage-based pricing](https://unity.com/products/gaming-services/pricing). Do not enable Unity billing as a substitute for Steam integration.

Steam identity can key later versioned saves; Steam Cloud/save migration and trusted competitive rewards are separate features, not automatically supplied by multiplayer. Keep publisher/server keys out of the repository/build. Player-hosted P2P does not provide a trusted competitive server, and no recurring multiplayer-service charge does not mean every publishing/backend cost is zero. Valve's example App ID 480 is for controlled development checks only, never the released game's production identity.

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
- Steam SDK lifecycle, friends-only two-player lobbies, numeric room codes, clipboard copying, invite overlay/startup invite support, and NGO P2P transport, with retained LAN mode. Duplicate connect clicks are blocked; canceled/late service requests cannot open an unwanted arena. Game/version checks and room-member validation protect the connection boundary. Actual two-user Steam P2P/invites remain unverified.
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

Networking uses Unity [Netcode for GameObjects 2.13.3](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/changelog/CHANGELOG.html), Unity Transport for LAN, and pinned [Steamworks.NET 2025.164.1](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2025.164.1) with a SteamNetworkingSockets P2P transport for Steam. Native reliable delivery and explicit unreliable sequencing carry gameplay messages; lobby membership and matching game/version are checked. No matchmaking queue, saved progression, or host migration is implemented. A player-hosted private prototype is authoritative over its clients, but is **not a trusted competitive/reward server**. Movement currently uses server simulation and transform interpolation, not client prediction; real-network latency/loss tests and performance budgets remain later work.

## Build and validate

Assets/scenes and their `.meta` files are checked in. **Tools > Tower Defense > Create Missing Prototype Assets** can create missing foundation assets; it deliberately does not overwrite existing authored assets. Open MainMenu afterward.

Use **Window > General > Test Runner > EditMode** for foundation, online-lifecycle, and build-publication tests (the lifecycle tests use a fake service, with no cloud traffic). Set **Project Settings > Player > Version** to the update number; `0.1` completes Step 1, `0.2` will complete Step 2, and fixes between milestones can use `0.1.1`, etc. The menu shows this version.

Choose **Tools > Tower Defense > Build Versioned Windows Player**, or run this in PowerShell, adjusting the Unity installation path if necessary:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath "$PWD\Tower Defense PVP" -executeMethod TowerDefense.Editor.PrototypeBuilder.BuildVersionedWindows -logFile "$PWD\Tower Defense PVP\Logs\prototype-build.log"
```

Close the editor before a command-line Unity run against the same project. The executable can return control to PowerShell before the build is finished; confirm `TD_PUBLISH_PASS` and `TD_BUILD_PASS` in the log. The older `BuildWindows`/`BuildInternetWindows` entry points now use this same versioned workflow. Rebuilding a version creates `-build2`, `-build3`, etc., never replacing the previous build. A successful build packages all playable files (not Unity's debug-backup folder), records `Builds/latest-build.json`, and updates the existing shortcut's target, working directory, and icon. Failed/canceled builds do not redirect it. Older `Prototype`/`InternetPrototype` folders are preserved. Use this build command rather than Unity's generic Build button for versioned packaging/shortcut updates.

### Publish a download on GitHub

Build, run the EditMode tests and standalone checks, add `Releases/<version>.md`, commit on the feature branch, and push that branch to origin. Then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\PublishBuild.ps1
```

This explicit command selects the latest build, requires a clean source checkout, repeats LAN checks against that exact executable, and uses Git's existing GitHub sign-in without saving credentials. It creates/resumes a draft at the pushed source commit, uploads and verifies the ZIP size/SHA256, then publishes `v<version>` as the latest release. It never merges into main or replaces an existing published release. Use a new version for another published update. `-DryRun` checks local artifact selection without signing in or changing GitHub. The shortcut updates for local builds; GitHub updates when this release command succeeds, not during every local experiment.

### Go back to an earlier update

Commits are snapshots of tracked source files. Branches are movable labels for different lines of work; creating/updating a feature branch does not erase previous commits or overwrite another branch. Release tags such as `v0.1` stay fixed at the released source snapshot, and each release keeps its own downloadable Windows ZIP. Generated Unity caches/build folders are not source snapshots; keep old build folders or download the corresponding release ZIP.

To **play** an older version, open [GitHub Releases](https://github.com/paubchud/Tower-Defense-Unity/releases) and download that version's Windows ZIP. To **continue coding** from an older version, first commit any work you want to keep, then create a new recovery branch rather than resetting/deleting current work:

```powershell
git fetch origin --tags
git switch -c codex/recovery-v0.1 v0.1
```

`main` stays unchanged while new increments are developed/tested on `codex/...` branches. Publish a new version/tag after an update passes its checks; do not move old release tags, force-push history, or replace old release downloads. If a merged update later needs undoing, a new revert commit can reverse it while retaining the history.

The development build has opt-in command-line smoke diagnostics: `-td-smoke-host` and `-td-smoke-client` use localhost port **7779**, automatically ready up, move, select items, and check replicated state/camera centering. Virtual input devices exercise the real WASD, orbit, wheel, and five panel bindings, including blocked gameplay while panels are open. They then reset, leave, and rejoin with swapped classes to check cleanup. These diagnostics are inactive during normal play and absent from release gameplay. A third instance with `-td-smoke-client -td-expect-reject`, launched while both players are connected, checks the two-player limit without resetting the accepted players. `-td-captures <existing-directory>` renders the live menu/arena/equipment UI to PNGs, including when hidden test windows skip screen presentation. Each process logs explicit `TD_*_PASS` or `TD_*_FAIL` results before exiting; consult each process's `-logFile` output.

After building, run `powershell -ExecutionPolicy Bypass -File .\Tools\ValidatePrototype.ps1 -Capture` from the repository root for LAN regression checks. It reads the latest-build manifest; `-ExecutablePath <exe>` can test an older build explicitly. Each run saves output in a separate ignored folder and only stops its own processes. `-Relay` is rejected on this branch; the old Relay runner is retained in `v0.1`. See [STEAM_SETUP.md](STEAM_SETUP.md) for the explicit private SDK diagnostic and the separate two-account/device acceptance checklist. A local LAN check or single-account SDK check does not prove Steam P2P gameplay. This invocation bypasses policy for this one process; it does not change the machine's execution-policy setting.

Verified on 2026-10-01 for the Steam preview: 36/36 automated tests, Windows build/package/shortcut, bundled Steam native DLL, and standalone LAN checks for movement/controls, two-player limit, reset, opponent disconnect, and swapped-class rejoin. The private App ID 480 diagnostic passed real SDK initialization, signed-in identity, private lobby creation/leave/fresh rehost. The actual game's App ID/entitlement, two-user P2P, friend invites, and two-home reachability remain unverified. The published 0.1 checkpoint previously passed 33/33 tests and LAN regression; its preceding Relay increment also passed live two-process Relay checks. No EOS integration was installed; that plan was canceled. Extract the whole Windows ZIP before launching.

Known rendering warning: the starter URP settings can report stripped, unused depth-of-field/Panini postprocessing shaders when first rendering a development build. The prototype does not use those effects; the lit 3D scene and UI render correctly. Revisit the postprocessing configuration when introducing visual effects rather than keeping unused shader variants solely to silence a warning.

## Design and next milestone

See [GAME_DESIGN.md](GAME_DESIGN.md) for the intended game and [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) for the implementation order. Next: configure the real Steam App ID and complete two-account/device verification before merging/publishing the Steam increment. Then Step 2 as version 0.2: player-sent units, basic hero/group-tower combat, separate match XP/gold, castle victory, and timed ghost respawn. Decide its initial rules before wiring those transactions.
