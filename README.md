# Tower Defense PVP

A 3D, hero-controlled 1v1 tower-defense prototype. Experimental **[0.3.1](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.3.1)** adds the first land, mining, tower construction/refund and match-only hero-growth slice on `codex/milestone-3-economy`; it is not the complete inventory/invasion game.

**Current release 0.3.1:** [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.3.1/TowerDefense-0.3.1-Windows.zip) / [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.3.1/TowerDefense-0.3.1-macOS.zip), fixed **v0.3.1 / d2b3b9e**. Both complete anonymous downloads passed size/SHA256 checks; 130/130 tests and repeated Windows host/client combat/economy/ghost/reset/rejoin/rejection passed. Mac verification is static and the app remains unsigned/unnotarized. Milestone 2 was checkpointed/pushed to main at `9e6abcd`; 0.3.1 stays on its pushed update branch for testing. The owner accepts networking as operational based on reported EOS/earlier Steam play; a detailed new platform/latency/balance pass is not implied.

See [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) for completed changes, test results, release checkpoints, and unfinished work. It is updated with each meaningful increment.

**Release policy (2026-10-03):** verified playable updates are published as versioned experimental GitHub Releases for owner/friend testing; 0.3.1 is now published. Publish matching Windows/Mac ZIPs, keep old tags/downloads fixed, and report any failed verification/publication rather than claiming a local update is downloadable. The release workflow pushes its source checkpoint to this public repository; main merges remain tied to completed milestone transitions. Documentation-only edits and unfinished experiments are not new playable releases.

**Owner checkpoint/testing policy (2026-10-03):** friend play is owner-reported successful for Windows Steam/EOS and Mac/Windows Steam; the latest EOS success report did not supply an exact version/platform/roles. This is basic play feedback, not a complete measured regression pass. Main was pushed at `a008f71` before Milestone 2 and `9e6abcd` before Milestone 3. The owner authorizes completed-branch main pushes at milestone transitions; the repository remains public. Versions use `0.<milestone>.<progress>`: current 0.3.1, next economy increment 0.3.2. Published releases/tags remain fixed.

Published experimental **0.1.5** connects the selected EOS guest backend: anonymous Connect Device ID login, TDG code-selected lobbies and an NGO EOS P2P/relay adapter, keeping automatic configured Steam sign-in and explicit guest fallback before the same menu. Local guest profile identity and authenticated EOS Product User ID are separate; no progression/cloud saves are implied. Steam and guest matchmaking remain separate. 92/92 tests and real single-device guest login/cancel/host/leave/fresh-rehost checks passed. Windows/Mac packaging, regression and anonymous-download checks are in the development log; actual guest two-device/home reachability and Mac execution still require testing. Queue/party contracts are foundations only, not enabled modes. See [EOS_SETUP.md](EOS_SETUP.md).

Previous published update: **[0.2.1 Windows/Mac experimental combat playtest](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.2.1)** stays fixed at `a5ae00e`; both complete anonymous ZIP downloads passed size/SHA256 checks. Its prior 110/110 tests and Windows combat/regressions passed; Mac package verification was static. That source is now contained in main's `9e6abcd` checkpoint. Older tags/downloads stay fixed, including [0.1.5](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5) at `855f4af`; stable **v0.1** is the legacy Unity Relay build. Explicit Steam development testing uses App ID 480, not our production identity.

**Design, not enabled gameplay:** [MULTIPLAYER_FLOW.md](MULTIPLAYER_FLOW.md) specifies planned solo/party queues, 1v1/2v2v2v2/4v4, pre-queue class/build choice, ready/loading gates and reconnect/leaving policy. This design is included in the main checkpoint; these are not shipped features. No visibility/history change has been made.

**Planned fog milestone:** [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) assigns fog of war to the start of Milestone 4, before invasion. Hidden enemy locations/stats are withheld from ordinary clients; ghosts grant no vision. Life-state/private-data groundwork belongs in Milestones 2-3. Fog, movement prediction and replacement-host recovery are not implemented in 0.1.5; host/checkpoint secrecy limitations remain explicit in [GAME_DESIGN.md](GAME_DESIGN.md) and MULTIPLAYER_FLOW.md.

## Open and play

1. In Unity Hub, open **`Tower Defense PVP`**, the project folder inside this repository. Use Unity **6000.6.3f1**.
2. Open `Assets/TowerDefense/Scenes/MainMenu.unity` and enter Play mode.
3. At startup, a valid Steam sign-in opens the main menu automatically. Until the game's own App ID is configured, use **Enable Private Steam Test (480)** on the startup screen (or the explicit private launcher), with Steam already signed in. Alternatively choose **Play as Guest** to enter the menu with your device-local profile. Select **Play**, then **Warrior** (Primate) or **Wizard** (Mystic). The Steam host chooses **Host Steam** and shares the numeric room code; their friend uses **Join Steam** on another account/device. See [STEAM_SETUP.md](STEAM_SETUP.md). The fixed older 0.1.3 download still enables test mode after class selection.
4. Both click **Ready**. Keep the host open and share codes privately. These are friends-only, two-player lobbies using Valve's shared Spacewar identity, not our production App ID. Prefer room codes; both must already be running this game for private-test invitations, or Steam can launch Spacewar instead.
5. In **0.3.1 guest play**, launch directly, choose **Play as Guest**, then Play/class/**Host Guest** or **Join Guest** with your friend's full **TDG** code. Both use matching 0.3.1 packages on separate devices and ready up. EOS login requires no Steam/Epic sign-in UI. Rooms are code-selected advertised EOS lobbies, not password-protected private rooms; no random queue is exposed. Guest and Steam players cannot join each other's matches. For **LAN / This PC**, two copies on this computer use `127.0.0.1`, port `7777`; on a LAN the joining player uses the host's LAN IPv4 address. Allow private-network firewall access if prompted. No saved progression/cloud recovery is implemented.

To launch the current local Windows game, use the folder in **Builds/latest-build.json** and open **TowerDefense.exe** for guest play. For Steam testing, run **Start-Private-Steam-Test.cmd** with Steam signed in, or explicitly enable test mode at startup. Mac builds use **Builds/latest-build-macOS.json**; transfer/extract the whole Mac ZIP on a Mac and follow [MAC_TESTING.md](MAC_TESTING.md). Builds keeps only the newest successfully packaged folder and ZIP **per platform**, plus validation logs. The owner removed the root shortcut and the builder will not recreate it. Builds remains ignored by Git; complete game ZIPs are attached to GitHub Releases separately from source. An experimental App ID 480 download is not our production Steam identity and does not change friends-only lobby privacy.

## Send it to a friend

Use the latest **[0.3.1 release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.3.1)**: [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.3.1/TowerDefense-0.3.1-Windows.zip) / [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.3.1/TowerDefense-0.3.1-macOS.zip). Extract everything. Both players need matching 0.3.1; guests launch directly, Steam tests use the explicit 480 launcher/startup option. On Mac follow MAC_TESTING.md for unsigned-build handling. Open B for land/build/refunds/growth; select pickaxe and press E near an own-side rock to mine. New Mac execution and real-peer economy/balance/latency checks remain open. Older fixed downloads below are recovery options, not the current version.

For recovery, **[0.2.1](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.2.1)** retains its [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.2.1/TowerDefense-0.2.1-Windows.zip) and [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.2.1/TowerDefense-0.2.1-macOS.zip), not source-code archives. Extract everything and use matching versions/accounts/devices. That version has combat but not purchased land/mining/building/hero growth. Its ghost/results fixtures require opt-in diagnostic flags; Mac is unsigned/unnotarized and full real-peer combat acceptance remains unmeasured. Other older versions below remain available for recovery.

For the new guest adapter, use the [0.1.5 release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5): [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.5/TowerDefense-0.1.5-Windows.zip) or [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.5/TowerDefense-0.1.5-macOS.zip), not the source-code archive. Extract everything; open the executable/app directly, choose Play as Guest, then Play/class/Host Guest or Join Guest with the full TDG code. Both need matching 0.1.5 on separate devices. Two copies under one Windows account share the EOS guest identity; use LAN / THIS PC with `127.0.0.1:7777` instead. The generic transport-start error and incorrect destroy-cleanup warning for self-connect are known follow-ups, not fixed here. Mac is unsigned/unnotarized and real Mac/two-device guest verification remains open. Steam's explicit test launcher and separate matchmaking are still available.

The following older releases are retained for recovery; do not mix versions between friends.

The currently published [v0.1 Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1) is the **legacy Unity Relay build**. Download its **Windows ZIP asset**, not GitHub's automatically generated source-code ZIP. Your friend extracts the whole ZIP and opens `TowerDefense.exe`. In that version only, use **Host Online / Join Online** with the room code, then both **Ready**. It requires neither Unity nor Steam, uses anonymous Unity sign-in, and has Relay service quotas. Both players must use the same Windows 64-bit build.

The 0.1.3 update uses one [experimental release page](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3) for **TowerDefense-0.1.3-Windows.zip** and **TowerDefense-0.1.3-macOS.zip**. See the development log for publication status. Download the ZIP for your platform under **Assets**, not the source-code ZIP. Both friends must use **0.1.3**, extract the whole ZIP and sign into separate Steam accounts/devices. Windows opens **Start-Private-Steam-Test.cmd**; Mac opens **TowerDefense.app** and enables test mode, following [MAC_TESTING.md](MAC_TESTING.md). The Mac prototype targets Intel and Apple Silicon/macOS 12+, but is not Developer ID signed/notarized and still needs a real Mac test. They need neither Unity nor GitHub collaborator access to a public release. No own-game App ID or Epic/Unity cloud setup is required for this test. This is still movement/lobby testing, not the complete combat/economy loop or verified Windows/Mac Steam peer play. Older [v0.1.2](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) remains available for recovery.

## One sign-in flow, separate matchmaking

One startup flow automatically uses valid configured Steam, otherwise offers an explicit guest choice. Source 0.1.5 guests authenticate through EOS Connect Device ID, independently of Steam, and use their own lobbies/relay transport. A persisted GUID under `Application.persistentDataPath/Accounts/guest-profile.json` remains the separate local profile key; the EOS Product User ID is `eos-guest` online identity, not this GUID or a Netcode client ID. Guests and Steam cannot match together. No cloud storage, recovery/account linking, unlock saving or trusted rewards are included; unreadable profiles are not silently replaced. No paid billing was enabled. [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md) and [EOS_SETUP.md](EOS_SETUP.md) distinguish implementation from pending peer tests.

The private increment implements Steamworks.NET, lobbies/invitations, a Netcode-compatible [Steam networking](https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay) transport, and explicit playable development App ID 480 mode. Real two-user P2P checks need two accounts/devices but not registration of our own App ID yet. Production distribution later requires it. **Steam integration is not in the published 0.1**; that build uses Unity Relay with service quotas. Do not enable Unity billing as a substitute for Steam/guest integration.

Provider-namespaced identity can key later versioned saves; Steam, Steam-test and guest keys do not collide. A profile key is not a network client ID or authentication proof. Steam Cloud, guest recovery/linking and trusted rewards are separate future work, not automatically supplied by multiplayer. Keep publisher/server keys out of the repository/build. Player-hosted P2P is not a trusted competitive server. Valve's App ID 480 is for explicit development testing only, never our game's production identity.

## Controls

| Control | Action |
| --- | --- |
| WASD | Move relative to the camera |
| Hold left mouse | Aim toward the ground cursor and attack with the weapon selected |
| Hold right mouse + drag | Orbit/tilt around the hero, always keeping the hero centered |
| Mouse wheel / click a hotbar slot | Select weapon, tool, or empty slot |
| E | Mine the nearest own-side stone node with the pickaxe selected; remain nearby for completion |
| B | Buy/sell land, build/sell a group tower, or buy match-only hero growth |
| U | Spend match XP to upgrade future troops |
| T | Open troop panel and send a Raider |
| I | Starter equipment and class information |
| Esc | Open/close pause menu; multiplayer itself does not pause |
| Menu button | Leave the session and return to class selection via the main menu |

The host's pause menu includes **Reset to Lobby**. Both players must ready again. Opening a management panel blocks local movement/orbit; scrolling over UI does not also change held items.

## Milestone 2 combat baseline (0.2.1)

- Main Menu, Store explanation, data-driven Warrior/Wizard selection, host/join setup, two-player ready lobby.
- Steam SDK lifecycle, friends-only lobbies, numeric codes, copying, invite support, NGO P2P transport and explicit playable private test mode, with retained LAN diagnostics. Duplicate/canceled requests, version/member checks and bounded pending callbacks protect the connection boundary. Actual two-user Steam P2P/invites remain unverified.
- Provider-neutral room/transport preparation and separate account-profile identity namespaces. Source/local packages 0.1.5 add anonymous EOS guest login, TDG rooms and a bounded independent relay transport; real two-device guest verification and saved progression remain pending. The older published 0.1.3 does not include EOS.
- Blockout 3D heroes with distinct starter weapon/clothing/tool visuals and a synchronized three-slot starter hotbar.
- A small authored-map definition with two straight lane paths, two castles, land markers, resource markers, and open space between sides. Markers do not yet transact or harvest.
- Server-authoritative movement, class/side assignment, readiness, active hotbar slot, and lobby reset. Only the owning player can submit their movement/selection requests. The host rejects a third connection and resets to the lobby when the opponent leaves.
- Independent local hero-follow/orbit cameras with scenery obstruction handling.
- Host-owned combat rules, bounded troop sending, XP upgrades for future sends, gold on kills, group-selected starter towers, castle defeat/draw/results, seven-second ghost/respawn, and energy-using sword/staff attacks. Troop visuals reuse a bounded pool and evaluate straight-path positions from spawn time; changed health/spawn/removal data is replicated instead of per-frame troop transforms. Private match economy and energy are owner-readable only. A bounded host-only transaction journal is diagnostic groundwork, not durable recovery or periodic auditing.

In the fixed older 0.2.1, purchased land/tower construction, harvesting/materials and gold spending were not present; the 0.3.1 slice below adds them. Persistent store stock, loot/crafting, theft, progression and fog remain future work. Walking across the blockout is not a finalized invasion rule. Ghosts roam their own half and can upgrade troops, but cannot attack/collect/reveal; sending while ghost defaults off pending owner choice. Existing towers continue firing. Disconnect still resets the lobby rather than preserving a match for reconnect; host loss still ends it.

### 0.3.1 economy slice (implemented, locally verified and experimentally released)

Each side has two authored plots and two stone nodes. Open **B** to choose an own-side plot/tower and buy, build, sell or grow your hero; land/build management works from anywhere on your side, including as a ghost. Select the pickaxe with the wheel, approach a labeled own-side rock and press **E** for one timed harvest. Moving away, changing tools, dying or match end cancels mining. Neither the UI nor client-submitted amounts decide authority costs/rewards. The server checks shared round/identity/command sequences, ownership, group eligibility, occupancy and complete costs before changing state. Private reserves/stone/harvest progress are owner-readable only; structure appearances are still public until Milestone 4 fog filtering.

Temporary editable **EconomyRules** defaults: 50 starting gold; land costs 25 gold and sells for 12; a group tower costs 20 stone and sells for 10 stone; stone capacity 100. Nodes hold 50 stone, provide 5 per two-second harvest within three units, and refill 20 seconds after depletion. One 30-gold purchase adds 25 maximum HP for this match only, without immediately healing; next respawn fills the upgraded maximum. Sell towers before land; make inventory room for refunds. Existing towers still earn kill gold during ghost life, but ghosts cannot mine. All land, towers, resources, growth and ongoing actions reset on a new match. These values are not final balance choices or persistent store upgrades. Full gear acquisition/crafting, trade between players and automated harvesting are not implemented.

## Content and code

Content assets are under `Assets/TowerDefense/Content`. `PrototypeCatalog` references classes and `StraightDuel`; each class references its technology group, starter items, and energy definitions. Adding a class does not require a new network message or a duplicate tower list. The current selection screen is laid out for the initial two classes; a paged/scrollable roster is needed before expanding to 25.

Warrior has a sword, mail, pickaxe and 100 stamina (8 per swing, 10/sec recovery). Wizard has a staff, robes, pickaxe and 100 mana (12 per shot, 8/sec recovery). The staff currently uses a narrow authority-checked instant ray with a cosmetic trace, not a traveling projectile. Attacks target sent troops on your own lane; hero-versus-hero/castle/tower attacks await invasion. These editable defaults are not final balance decisions.

`CombatRules` defines the temporary loop: free Raider send every 1.5 seconds, +5 XP per accepted send, 25 XP for one upgrade, 10 gold per kill to the lane defender, 16 troops per lane, 300 castle HP, and seven-second respawn. Upgrades affect future sends only. Gold accumulates for testing but cannot yet be spent. Ghosts retain economy, regenerate no energy while dead and respawn at home with starting health/energy. Match end stops combat and respawn; host can reset for a fresh rematch.

Map lane endpoints/spawns, plot positions, and resource positions are editable in `StraightDuel`. Runtime geometry currently builds a simple blockout from those positions; it is not the final map-art workflow.

Runtime folders separate Data, Core rules, Input, Networking, Presentation, UI, and development-only Diagnostics. Editor generation/build tooling and NUnit tests have separate assemblies.

Networking uses Unity [Netcode for GameObjects 2.13.3](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/changelog/CHANGELOG.html), Unity Transport for LAN, and pinned [Steamworks.NET 2025.164.1](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2025.164.1) with a SteamNetworkingSockets P2P transport for Steam. Native reliable delivery and explicit unreliable sequencing carry gameplay messages; lobby membership and matching game/version are checked. No matchmaking queue, saved progression, or host migration is implemented. A player-hosted private prototype is authoritative over its clients, but is **not a trusted competitive/reward server**. Movement currently uses server simulation and transform interpolation, not client prediction; real-network latency/loss tests and performance budgets remain later work.

## Build and validate

Before opening a fresh checkout, run `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/InstallEosSdk.ps1`. This installs the checksum-pinned official C# SDK/native libraries into an ignored local package; it requires no account credentials to download. Configure `.local/eos-settings.json` locally as described in [EOS_SETUP.md](EOS_SETUP.md). Never put secrets in chat/Git. Downloadable clients necessarily contain extractable, least-privilege game-client credentials; no admin/server credentials may be used. Assets/scenes and their `.meta` files are checked in. **Tools > Tower Defense > Create Missing Prototype Assets** creates missing foundation assets without overwriting authored assets.

Use **Window > General > Test Runner > EditMode** for combat, foundation, online-lifecycle and build-publication tests (lifecycle tests use a fake service, with no cloud traffic). Set **Project Settings > Player > Version** to `0.<milestone>.<progress>`: the first Milestone 2 increment is `0.2.1`, the next `0.2.2`, and Milestone 3 starts `0.3.1`. Existing historical tags are unchanged. The menu shows this version; starting a milestone does not mean its full gate has passed.

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

See [GAME_DESIGN.md](GAME_DESIGN.md), [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) and [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md). The owner accepts connectivity as operational and requested the next gameplay step: 0.3.1 land/harvest/build/hero-growth work. LAN automation uses `Tools/ValidatePrototype.ps1 -Combat -Economy -Capture`. Detailed real-peer combat/economy, Mac/Windows EOS, measured latency/loss/load and balance checks remain open. Prediction, fog and host recovery remain explicitly separate work. Use a real App ID before production Steam publication.
