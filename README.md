# Tower Defense PVP

A 3D, hero-controlled 1v1 tower-defense prototype. The first implementation is the multiplayer foundation, not the complete combat/economy game.

See [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) for completed changes, test results, release checkpoints, and unfinished work. It is updated with each meaningful increment.

## Open and play

1. In Unity Hub, open **`Tower Defense PVP`**, the project folder inside this repository. Use Unity **6000.6.3f1**.
2. Open `Assets/TowerDefense/Scenes/MainMenu.unity` and enter Play mode.
3. Select **Play**, choose **Warrior** (Primate) or **Wizard** (Mystic), then **Host Online**. The lobby displays a room code; **Copy Room Code** copies it.
4. Your friend selects a class, enters that code, and clicks **Join Online**. Both click **Ready**. The host must keep the game open. Share codes privately; anyone with the code can attempt to join the two-player room. Leaving the host session invalidates its code; hosting again creates a fresh one.
5. For local-only testing, choose **LAN / This PC** instead. Two copies on this computer use `127.0.0.1`, port `7777`; on a LAN the joining player uses the host PC's LAN IPv4 address. Both use the same port; allow the game through the private-network firewall if prompted. LAN play does not initialize the online services.

Double-click the repository's **TowerDefense.exe.lnk** shortcut to play the newest successfully packaged Windows build. Step 1 is **version 0.1**, in `Builds/TowerDefense-0.1-Windows/TowerDefense.exe`. The builder automatically updates this same shortcut; old builds stay intact. Builds and validation output are intentionally ignored by Git, while downloadable ZIPs are published as GitHub Releases.

## Send it to a friend

Share the [latest GitHub Release](https://github.com/paubchud/Tower-Defense-Unity/releases/latest), or send `Builds/TowerDefense-0.1-Windows.zip`. Download the **Windows ZIP asset**, not GitHub's automatically generated source-code ZIP. Your friend extracts the whole ZIP, opens `TowerDefense.exe`, and follows the online steps above. They do not need Unity, GitHub, Steam, or a Unity account. Both players must use the same build; this build is Windows 64-bit only. Internet play currently uses Unity Relay, so the host does not need to share an IP address or forward router ports. Local gameplay, not the complete combat/economy loop, is available in this prototype.

The Unity project is already linked to a cloud project. For a different/forked project, link it in Unity's Project Settings > Services and enable Authentication/Relay in that project's Unity Dashboard before building. Do not put service-account keys in the game or repository. The runtime uses anonymous guest sign-in, encrypted DTLS transport, and the default service environment. It keeps separate cached guest profiles for simultaneous copies on one PC. Profiles are a testing convenience, not permanent progression accounts. If the service is unavailable, the menu explains the failure and allows retry/cancel or LAN play. No paid service plan is enabled by this code; monitor the project's service usage before a public release.

## Steam and itch publishing plan

**Itch first for public feedback, Steam later.** Testers must be able to play as guests without signing into Steam or an Epic account. No port forwarding and no metered multiplayer-service usage charges are requirements. Cross-play with Steam is optional, not a release requirement.

Planned next networking step: Epic Online Services (EOS) Connect device-ID guest identity and P2P/relay, retaining host-authoritative gameplay and LAN testing. [EOS's licensing](https://onlineservices.epicgames.com/licensing) offers its services without royalty or hosting fees. Integration still requires an Epic developer product/deployment/client configuration, SDK/transport integration, service-policy compliance, and real two-machine tests. **EOS is not implemented in 0.1**; this build still uses Unity Relay, which has a free allowance followed by [usage-based pricing](https://unity.com/products/gaming-services/pricing). Do not enable billing or treat the current Relay build as the agreed permanent no-usage-charge solution.

Add optional account linking before durable saved progression: a guest/device identity is not a cross-device recovery account. Local prototype saves can come first, but trusted competitive currency/results need separate authority. Later add Steam sign-in/invites once a Steamworks App ID is available. A separate Steam-only transport is acceptable; do not force itch testers into Steam login. Keep service secrets out of the repository/build; player-hosted P2P does not provide a trusted competitive server or guarantee that future backend/publishing costs are zero.

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
- Internet host/join through Unity Relay with shareable room codes, clipboard copying, anonymous authentication, and retained LAN mode. Duplicate connect clicks are blocked; canceled/late service requests cannot open an unwanted arena. Invalid/expired codes and service failures show recoverable messages.
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

Networking uses Unity [Netcode for GameObjects 2.13.3](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/changelog/CHANGELOG.html), Unity Transport, and Multiplayer Services SDK 2.3.3's [Relay integration](https://docs.unity.com/en-us/mps-sdk/tutorials/relay-and-ngo). Relay carries the existing gameplay messages; it does not create a trusted server, matchmaking, account progression, or host migration. A player-hosted private prototype is authoritative over its clients, but is **not a trusted competitive/reward server**. Movement currently uses server simulation and transform interpolation, not client prediction; latency testing and performance budgets remain later work.

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

After building, run `powershell -ExecutionPolicy Bypass -File .\Tools\ValidatePrototype.ps1 -Capture` from the repository root for the LAN regression checks. It reads the latest-build manifest; `-ExecutablePath <exe>` can test an older build explicitly. Add `-Relay` to exercise the actual online menu, cloud allocation, code entry/copying, movement, reset, and fresh-code rehosting. The Relay run contacts Unity's live services and uses their quotas. It is two peers on this PC communicating through Relay, not a substitute for testing from two separate internet connections. Each run saves output in a separate ignored folder and only stops its own processes. Its transient code file contains only the shareable room code, never allocation keys or tokens. This invocation bypasses policy for this one process; it does not change the machine's execution-policy setting.

Verified on 2026-10-01: 33/33 automated tests, version 0.1 Windows build/package/shortcut, and standalone LAN checks with both player roles, movement/controls, two-player limit, reset, opponent disconnect, and rejoining with swapped classes. The preceding internet increment also passed live Unity Relay checks; online connection/lobby screenshots were visually checked then. Live Relay was not repeated merely for version naming/packaging. A two-machine/two-home internet test, EOS, and Steam SDK integration remain unverified/not implemented. `Builds/TowerDefense-0.1-Windows.zip` contains the playable Windows files, excluding Unity's debug-backup folder; extract it before launching.

Known rendering warning: the starter URP settings can report stripped, unused depth-of-field/Panini postprocessing shaders when first rendering a development build. The prototype does not use those effects; the lit 3D scene and UI render correctly. Revisit the postprocessing configuration when introducing visual effects rather than keeping unused shader variants solely to silence a warning.

## Design and next milestone

See [GAME_DESIGN.md](GAME_DESIGN.md) for the intended game and [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) for the implementation order. Before a public itch multiplayer test, configure and integrate EOS guest/P2P and test two separate machines/connections. The next gameplay milestone remains player-sent units, basic hero/group-tower combat, separate match XP/gold, castle victory, and timed ghost respawn. Decide its initial rules before wiring those transactions.
