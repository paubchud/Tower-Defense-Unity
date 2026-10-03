# Tower Defense PVP

A 3D, hero-controlled 1v1 tower-defense prototype. The first implementation is the multiplayer foundation, not the complete combat/economy game.

See [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) for completed changes, test results, release checkpoints, and unfinished work. It is updated with each meaningful increment.

Current update: **0.1.3 Windows/Mac experimental Steam playtest**, on `codex/macos-playtest`; main remains the owner's caught-up 0.1.2 source until another merge is authorized. See the development log for actual build/publication checks. All prior commits and fixed releases remain. Explicit development mode uses Valve's test App ID 480 without registering the game yet. Real two-account/device P2P verification remains required; Mac packaging checks are not Mac runtime verification. Guest play is planned and the provider/identity boundary is prepared, not a functioning guest backend. Retained stable **v0.1** is a different, older Unity Relay build.

## Open and play

1. In Unity Hub, open **`Tower Defense PVP`**, the project folder inside this repository. Use Unity **6000.6.3f1**.
2. Open `Assets/TowerDefense/Scenes/MainMenu.unity` and enter Play mode.
3. Select **Play**, choose **Warrior** (Primate) or **Wizard** (Mystic), then **Enable Private Steam Test (480)**. Sign into Steam first. The host chooses **Host Steam** and copies the numeric room code; their friend enables the same private mode on another Steam account/device and uses **Join Steam**. See [STEAM_SETUP.md](STEAM_SETUP.md).
4. Both click **Ready**. Keep the host open and share codes privately. These are friends-only, two-player lobbies using Valve's shared Spacewar identity, not our production App ID. Prefer room codes; both must already be running this game for private-test invitations, or Steam can launch Spacewar instead.
5. For **LAN / This PC**, two copies on this computer use `127.0.0.1`, port `7777`; on a LAN the joining player uses the host PC's LAN IPv4 address. Both use the same port; allow the game through the private-network firewall if prompted. LAN remains available without Steam. Guest internet/login/saves are future work; EOS is not enabled.

To launch the current local Windows game, use the folder in **Builds/latest-build.json** and run **Start-Private-Steam-Test.cmd** with Steam signed in. Or open **TowerDefense.exe**, then enable the Steam test mode in the menu. Mac builds use **Builds/latest-build-macOS.json**; transfer/extract the whole Mac ZIP on a Mac and follow [MAC_TESTING.md](MAC_TESTING.md). Builds keeps only the newest successfully packaged folder and ZIP **per platform**, plus validation logs. The owner removed the root shortcut and the builder will not recreate it. Builds remains ignored by Git; complete game ZIPs are attached to GitHub Releases separately from source. An experimental App ID 480 download is not our production Steam identity and does not change friends-only lobby privacy.

## Send it to a friend

The currently published [v0.1 Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1) is the **legacy Unity Relay build**. Download its **Windows ZIP asset**, not GitHub's automatically generated source-code ZIP. Your friend extracts the whole ZIP and opens `TowerDefense.exe`. In that version only, use **Host Online / Join Online** with the room code, then both **Ready**. It requires neither Unity nor Steam, uses anonymous Unity sign-in, and has Relay service quotas. Both players must use the same Windows 64-bit build.

The 0.1.3 update uses one [experimental release page](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3) for **TowerDefense-0.1.3-Windows.zip** and **TowerDefense-0.1.3-macOS.zip**. See the development log for publication status. Download the ZIP for your platform under **Assets**, not the source-code ZIP. Both friends must use **0.1.3**, extract the whole ZIP and sign into separate Steam accounts/devices. Windows opens **Start-Private-Steam-Test.cmd**; Mac opens **TowerDefense.app** and enables test mode, following [MAC_TESTING.md](MAC_TESTING.md). The Mac prototype targets Intel and Apple Silicon/macOS 12+, but is not Developer ID signed/notarized and still needs a real Mac test. They need neither Unity nor GitHub collaborator access to a public release. No own-game App ID or Epic/Unity cloud setup is required for this test. This is still movement/lobby testing, not the complete combat/economy loop or verified Windows/Mac Steam peer play. Older [v0.1.2](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) remains available for recovery.

## Steam now, guest play later

The owner chose **Steam development testing now, a public experimental test download, and guest play later**. EOS remains canceled; no guest backend or billing is enabled. Current internet players use signed-in Steam accounts, lobbies and P2P relay. [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md) records how to add independent guest identity, saves and networking later; guests must not need Steam. Cross-play is not required, and no unlimited/free anonymous relay guarantee has been made.

The private increment implements Steamworks.NET, lobbies/invitations, a Netcode-compatible [Steam networking](https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay) transport, and explicit playable development App ID 480 mode. Real two-user P2P checks need two accounts/devices but not registration of our own App ID yet. Production distribution later requires it. **Steam integration is not in the published 0.1**; that build uses Unity Relay with service quotas. Do not enable Unity billing as a substitute for Steam/guest integration.

Provider-namespaced identity can key later versioned saves; Steam, Steam-test and guest keys do not collide. A profile key is not a network client ID or authentication proof. Steam Cloud, guest recovery/linking and trusted rewards are separate future work, not automatically supplied by multiplayer. Keep publisher/server keys out of the repository/build. Player-hosted P2P is not a trusted competitive server. Valve's App ID 480 is for explicit development testing only, never our game's production identity.

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
- Steam SDK lifecycle, friends-only lobbies, numeric codes, copying, invite support, NGO P2P transport and explicit playable private test mode, with retained LAN diagnostics. Duplicate/canceled requests, version/member checks and bounded pending callbacks protect the connection boundary. Actual two-user Steam P2P/invites remain unverified.
- Provider-neutral room/transport preparation and separate account-profile identity namespaces. A tested non-Steam fixture can host via LAN without initializing Steam; this is preparation, not a shipped guest backend or saved progression.
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

Close the editor before a command-line Unity run against the same project. The executable can return control to PowerShell before the build is finished; confirm `TD_PUBLISH_PASS` and `TD_BUILD_PASS` in the log. The older `BuildWindows`/`BuildInternetWindows` entry points use this same workflow. A rebuild uses an unused versioned path rather than overwriting the currently working folder. Only after packaging, manifest promotion and any existing-shortcut update succeed does the builder remove older generated folders/ZIPs. Failed/canceled builds retain the working version. Validation logs and unrelated files are preserved; linked paths are skipped. Removed shortcuts are not recreated. Use this command rather than Unity's generic Build button for versioned packaging and cleanup.

### Mac build and package

Install **Mac Build Support (Mono)** in Unity Hub for 6000.6.3f1. Choose **Tools > Tower Defense > Build Versioned Mac Player (Universal Mono)**, or use the Windows command above with `-buildTarget OSXUniversal -executeMethod TowerDefense.Editor.PrototypeBuilder.BuildVersionedMac`. This explicitly uses Mono and Intel + Apple Silicon; macOS IL2CPP needs a Mac/Xcode. The packager checks the bundle version and Universal player/Unity/Steam libraries, preserves Unix executable ZIP modes and promotes `latest-build-macOS.json` separately from Windows. It refuses filesystem links and ZIP64 archives instead of following links or silently losing permissions. macOS 12+ is targeted; Intel support is deprecated in Unity 6.6 but still available for this test.

Run `powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\ValidateMacBuild.ps1` for bundle/native architecture, every packaged file hash and ZIP-permission checks. These are **static checks only**, not Mac launch, signing, performance or Steam peer verification. Follow [MAC_TESTING.md](MAC_TESTING.md) on a real Mac; normal trusted public Mac distribution needs signing/notarization later.

### Publish a download on GitHub

Build, run the EditMode tests and standalone checks, add `Releases/<version>.md`, commit on the feature branch, and push that branch to origin. Then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\PublishBuild.ps1
```

This explicit command selects the latest build, requires a clean source checkout, repeats LAN checks against that exact executable, and uses Git's existing GitHub sign-in without saving credentials. It creates/resumes a draft at the pushed source commit, uploads and verifies the ZIP size/SHA256, then publishes `v<version>` as the latest release. It never merges into main or replaces an existing published release. Use a new version for another published update. `-DryRun` checks local artifact selection without signing in or changing GitHub. The shortcut updates for local builds; GitHub updates when this release command succeeds, not during every local experiment.

Normal production publication still rejects Steam App ID 0/480. For an explicitly authorized public **experimental Steam test prerelease** only, use `Tools/PublishBuild.ps1 -SteamTestPrerelease` (optionally `-DryRun` first). This requires a development build with the explicit test launcher, labels it experimental, sets GitHub's prerelease flag and leaves the latest stable release unchanged. It does not configure Steam depots, change lobby privacy, merge main, or claim remote two-user verification. The owner authorized this route for 0.1.2; see [Releases/0.1.2.md](Releases/0.1.2.md).

For the owner-authorized paired Windows/Mac experimental release, build both at the same version, then use **`Tools/PublishBuild.ps1 -SteamTestPrerelease -IncludeMacOS`**. This verifies the Mac package statically and regresses the exact Windows executable, uploads/verifies both ZIPs before publishing one draft, and retains stable/older releases. A mismatched or invalid Mac package blocks paired publication. `-DryRun` also runs the static Mac checks without authentication/upload. Publication is not a Mac runtime or two-user Steam pass.

### Go back to an earlier update

Commits are snapshots of tracked source files. Branches are movable labels for different lines of work; removing a merged branch does not erase commits still reachable from main or release tags. Release tags such as `v0.1` stay fixed at the released source snapshot, and each release keeps its own downloadable Windows ZIP. Only the latest build is kept locally; recover published older games from GitHub Releases, or rebuild an unpublished source checkpoint.

To **play** an older version, open [GitHub Releases](https://github.com/paubchud/Tower-Defense-Unity/releases) and download that version's Windows ZIP. To **continue coding** from an older version, first commit any work you want to keep, then create a new recovery branch rather than resetting/deleting current work:

```powershell
git fetch origin --tags
git switch -c codex/recovery-v0.1 v0.1
```

Use temporary `codex/...` branches while developing future increments. Merge only when authorized, then remove completed branches whose history is preserved in main. The owner explicitly requested the current main catch-up despite remote Steam peer testing still pending. Do not move release tags, force-push history or replace old downloads. A new revert commit can undo a merged update while retaining history.

The development build has opt-in command-line smoke diagnostics: `-td-smoke-host` and `-td-smoke-client` use localhost port **7779**, automatically ready up, move, select items, and check replicated state/camera centering. Virtual input devices exercise the real WASD, orbit, wheel, and five panel bindings, including blocked gameplay while panels are open. They then reset, leave, and rejoin with swapped classes to check cleanup. These diagnostics are inactive during normal play and absent from release gameplay. A third instance with `-td-smoke-client -td-expect-reject`, launched while both players are connected, checks the two-player limit without resetting the accepted players. `-td-captures <existing-directory>` renders the live menu/arena/equipment UI to PNGs, including when hidden test windows skip screen presentation. Each process logs explicit `TD_*_PASS` or `TD_*_FAIL` results before exiting; consult each process's `-logFile` output.

After building, run `powershell -ExecutionPolicy Bypass -File .\Tools\ValidatePrototype.ps1 -Capture` from the repository root for LAN regression checks. It reads the latest-build manifest; `-ExecutablePath <exe>` can test an older build explicitly. Each run saves output in a separate ignored folder and only stops its own processes. `-Relay` is rejected on this branch; the old Relay runner is retained in `v0.1`. See [STEAM_SETUP.md](STEAM_SETUP.md) for the explicit private SDK diagnostic and the separate two-account/device acceptance checklist. A local LAN check or single-account SDK check does not prove Steam P2P gameplay. This invocation bypasses policy for this one process; it does not change the machine's execution-policy setting.

Verified on 2026-10-01 for 0.1.2: 47/47 automated tests, Windows build/package/shortcut/private launcher, native private Steam menu/lobby/P2P-listen/NGO-host/code-copy/leave/fresh-rehost check, normal unconfigured-mode guard and standalone LAN regression. The private menu/lobby captures were visually checked. This proves a real Steam host with one account, not remote two-user P2P or guest internet play. Two-account/device replication, friend invites and separate-home reachability still need testing. The prior 0.1.1 preview passed 36 tests; published 0.1 passed 33 tests and LAN regression, with preceding live two-process Relay checks. No EOS integration was installed. Extract the whole Windows ZIP before launching.

Known rendering warning: the starter URP settings can report stripped, unused depth-of-field/Panini postprocessing shaders when first rendering a development build. The prototype does not use those effects; the lit 3D scene and UI render correctly. Revisit the postprocessing configuration when introducing visual effects rather than keeping unused shader variants solely to silence a warning.

## Design and next milestone

See [GAME_DESIGN.md](GAME_DESIGN.md), [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) and [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md). Next: complete two-account/device Steam verification; the owner-authorized main catch-up does not close that test gate. Use a real App ID before production Steam publication. Then Step 2 remains version 0.2: sent units, hero/group-tower combat, match XP/gold, castle victory and timed ghost respawn. Decide its initial rules before wiring those transactions.
