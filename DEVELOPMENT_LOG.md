# Tower Defense PVP - Development Log

This is the progress record: what has actually been added, what has been tested, and what is still unfinished. Dates use the owner's America/Chicago timezone. New updates go at the top; older entries remain available.

The implementation order lives in [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md), the intended gameplay in [GAME_DESIGN.md](GAME_DESIGN.md), and play/download instructions in [README.md](README.md). A planned feature is not a completed feature.

## Current status

- **Step 1 / version 0.1:** released on GitHub as fixed tag `v0.1`, source checkpoint `881ee16`. [Download/release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1). Its Windows ZIP passed size/SHA256 verification and standalone regression.
- **Local launcher:** the existing `TowerDefense.exe.lnk` is now updated automatically by the versioned build command, after successful packaging. Older builds are retained.
- **GitHub recovery:** each published version will have a fixed `v<version>` tag and retained downloadable ZIP. Work stays on separate update branches; commits do not erase earlier snapshots.
- **Current networking work:** Steam-only 0.1.1 preview on `codex/steam-integration`. SDK/lobby/P2P integration is implemented. All 36 automated tests, final Windows packaging, LAN regression, private native SDK/lobby check, and unset-App-ID menu/retry check passed. Own-game App ID and real two-account/device multiplayer verification remain pending. EOS is canceled; released 0.1 still uses Unity Relay.
- **Next gameplay release:** 0.2, Step 2's combat and troop sending, after the Steam networking increment is verified, merged, and pushed.

## 2026-10-01 - Steam integration preview 0.1.1

Status: implemented; automated tests passed; not a released or fully verified Steam multiplayer update.
Branch: `codex/steam-integration`. Based on the fixed `v0.1` checkpoint; main remains unchanged.

### Added

- Pinned Steamworks.NET 2025.164.1 dependency, Steam settings asset, and Steam SDK lifecycle handling.
- Steam account-based, friends-only two-player lobbies, numeric codes, version/game matching, friend-invite overlay, and received/startup invite codes.
- NGO SteamNetworkingSockets P2P transport, room-member checks, native reliable delivery, and explicit unreliable sequence/duplicate filtering.
- Packet size is bounded by the SDK's actual send limit; pending native room requests are capped so rapid cancel/retry cannot grow retained callbacks without limit.
- Canceled/late room cleanup, host-loss handling without authority migration, and immediate socket cleanup on leave/rehost.
- Game-version validation in connection approval, separate from class selection.
- Steam host/join/invite menu actions, retained LAN/This PC diagnostics, and an explicit private SDK-only test using Valve's example app.
- A separate opt-in no-App-ID menu/retry diagnostic; normal play never silently uses Valve's sample identity. The SDK-generated local sample file is ignored by Git.
- Steam App ID 0 and 480 release-publication guards; Steam setup and two-device acceptance checklist in [STEAM_SETUP.md](STEAM_SETUP.md).
- Removed active Unity Relay authentication/connector/code and Multiplayer Services dependency from this branch. These remain recoverable in `v0.1`/the original branch; no billing was enabled.
- Release helper now reports the post-publication download URL rather than its temporary draft asset URL.

### Verified so far

- Unity imported the pinned SDK and compiled the integration.
- 36/36 automated tests passed: game foundations, Steam lobby-code validation, fake-service cancellation/retry/error handling, packet offset/order/duplicate/wraparound checks, and build publication protections.
- No live Steam service is called by the fake-service tests; the real SDK's private diagnostic is separate.
- Initial 0.1.1 Windows development build/package succeeded (`TD_BUILD_PASS`/`TD_PUBLISH_PASS`). The ZIP includes `steam_api64.dll` and the Steamworks.NET managed library; no `steam_appid.txt` or Unity do-not-ship backup is included. The shortcut and manifest target the preview, with Steam App ID zero recorded.
- Standalone LAN regression passed against the explicit 0.1.1 executable: host/client/third-player checks, controls, reset, disconnect, and swapped-class rejoin. Output: `Builds/Validation/Smoke-20261001-205823`. Menu/arena/equipment captures were visually checked.
- Real private Steam SDK diagnostic passed on this developer account using App ID 480: initialization, signed-in identity, private lobby create/leave/fresh rehost (`Builds/Validation/steam-private.log`). It did not invite/message anyone and does not prove two-player P2P.
- PowerShell syntax checks passed, and `PublishBuild.ps1 -DryRun` rejected the App ID zero preview before authentication/upload. No Steam preview release was published.
- Final recheck after packet/request limits: 36/36 tests passed (`Builds/Validation/steam-tests-final.xml`); versioned build succeeded without overwriting the first preview, producing `TowerDefense-0.1.1-Windows-build2`. Its ZIP dependency/exclusion check passed and the shortcut targets this executable.
- Local final preview ZIP SHA256: `efde8df0577452af54a7203d313efe8aa7c94b797f13b6b2a205258f24da2b8f`. This is not a published release asset.
- Final build LAN host/client/third checks all passed in `Builds/Validation/Smoke-20261001-210714`.
- Final private SDK/lobby and no-App-ID menu/retry diagnostics both passed in `Builds/Validation/Steam-final-20261001`. The latter verified no native initialization and a usable setup error/retry; its live connection-menu capture was visually checked.

### Not verified / remaining

- The game's own App ID, entitlement/distribution setup, two Steam accounts/devices, real NGO P2P replication, invites, and separate-home testing are not verified.
- No Steam Cloud/progression, achievements, matchmaking queue, host migration, or Steam depot upload.
- Do not merge into main or publish this preview as Steam-ready until the real App ID/multiplayer gate passes. Then start gameplay Step 2 as 0.2.

## 2026-10-01 - Direction changed to Steam only

Status: confirmed design change; integration not yet implemented.

- The owner canceled EOS and requested Steam-only compatibility. No EOS SDK was installed, no Epic product was created, and no EOS game code needs to be removed.
- Plan Steam account identity and Steam lobby/P2P relay networking; retain local LAN diagnostics. Guest itch play and cross-store multiplayer are no longer requirements for the next networking increment.
- Preserve version 0.1's source/build checkpoint before starting Steam on a separate branch. Do not label Steam networking verified until its SDK, App ID, and multiplayer checks are actually ready.
- The release helper's first run consumed excessive memory and was stopped without creating a release/draft. Bounded streaming fixed the path; the retry successfully published and verified `v0.1`.

## 2026-10-01 - Version 0.1 packaging and release workflow

Status: implemented, verified, and released as `v0.1`.
Branch: `codex/internet-join-codes`.
Released source checkpoint: `881ee16`; [release/download](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1).

### Added

- Step 1's update number is now `0.1`, displayed in the menu.
- Windows builds and shareable ZIPs are named by version, starting with `TowerDefense-0.1-Windows`.
- Rebuilding the same version uses a separate `-build2`, `-build3`, etc. folder rather than replacing the previous copy.
- The same repository shortcut automatically targets the newest successfully packaged build, with the correct working directory and icon. Failed/canceled builds do not redirect it.
- Packaging includes the playable executable, data, and runtime dependencies; Unity's do-not-ship debug backup is excluded.
- A latest-build record lets standalone validation and release publishing select the current build.
- An explicit GitHub release command runs LAN checks, uploads a ZIP into a draft, verifies its size/SHA256, and only then publishes it. Existing published versions are not replaced, and local experiments do not upload automatically.
- Version 0.1 release notes, rollback instructions, this log, and repository instructions to maintain this log during future work.
- The plan now records itch-first guest testing, EOS as the intended no-metered-multiplayer-service path, optional Steam/itch cross-play, and Steam integration later.

### Verified so far

- 33/33 Unity automated tests passed: existing foundation/online-lifecycle checks plus version validation, non-overwriting build names, ZIP contents, launcher/manifest publication, and failed-publication preservation.
- Versioned Windows development build and ZIP completed successfully.
- Windows shortcut helper ran successfully during the actual build.
- All PowerShell tooling passed syntax checks.
- The actual shortcut points to `Builds/TowerDefense-0.1-Windows/TowerDefense.exe`; its working directory is correct.
- The ZIP contains the executable, Unity runtime, and data, with no Unity do-not-ship debug-backup entries. Release selection passed a no-upload dry run.
- Standalone host, client, and third-player checks passed for this exact 0.1 build: controls, synchronized state, reset, disconnect, rejoin, and two-player limit. Captures saved in `Builds/Validation/Smoke-20261001-200836`.
- GitHub latest-release endpoint confirms public `v0.1` at source `881ee16`. The Windows ZIP is 73,178,197 bytes; its server SHA256 matches the local archive. The publishing retry repeated host/client/third checks successfully in `Builds/Validation/Smoke-20261001-202259`.

### Still to finish

- Start Steam integration on a separate branch; confirm the game's App ID, then implement and verify identity, lobby discovery/invites, P2P relay, cleanup, and two-player limits before merging. The earlier EOS plan was canceled.

## 2026-10-01 - Internet room-code multiplayer

Status: implemented and tested; part of the 0.1 network prototype.
Branch: `codex/internet-join-codes`.
Source checkpoint: `305639a`.

### Added

- Host Online / Join Online using private room codes and anonymous Unity guest sign-in.
- Copy-room-code button, recoverable service/code errors, and retained LAN / This PC play.
- Protection against duplicate connect clicks and canceled service requests starting a late/unwanted match.
- Separate cached guest identities for simultaneous game copies on one PC.
- Fresh codes on rehosting, two-player limit, and disconnect/reset/rejoin handling.

### Verified

- 18/18 automated foundation and online-lifecycle tests passed.
- Windows build, LAN regression, and live Unity Relay regression passed with both player roles, controls, third-player rejection, reset, disconnect, and rehosting with swapped classes.
- Online menu/lobby captures were visually checked.

### Limitations

- Live Relay testing used two peers on this PC, not two separate homes/machines.
- Unity Relay has service quotas; this is not the final EOS/no-usage-charge integration.
- No saved progression, Steam login/invites, matchmaking, trusted reward backend, or host migration.

## 2026-10-01 - Step 1: first two-player foundation

Status: implemented and tested.
Source checkpoint: `079e2d2`.

### Added

- Main Menu with Play, Store explanation, and Exit; Warrior/Wizard selection and a ready lobby.
- Small 3D blockout map with two straight lanes, castles, plot/resource markers, and space between sides.
- Two networked heroes, server-owned movement/class/side/readiness/item selection, and third-player rejection.
- Camera-relative WASD and independent, always hero-centered cameras with right-drag orbit/obstruction handling.
- Starter weapon, tool, and clothing visuals; a three-slot mouse-wheel hotbar.
- B shop, U upgrades, T troop sending, I equipment, and Esc pause/menu placeholders; UI blocks conflicting movement/orbit/item input.
- Class/group/shared-tower definitions, generic energy-pool rules, and isolated starter inventory containers.
- Reset-to-lobby, leave/menu cleanup, disconnect handling, and repeatable standalone diagnostics.

### Verified

- 6/6 initial foundation tests passed.
- Two standalone players passed movement, class/side/item synchronization, camera/input checks, reset/re-ready, leave/rejoin with swapped classes, and third-player rejection.
- Menu, arena, and equipment captures were visually checked.

### Not implemented by Step 1

Hero attacks, troop sending, working towers, XP/gold/material transactions, land purchasing, harvesting, invasion theft, ghost respawn, castle victory, progression/unlocks, loot/crafting, and final art. The map markers and management screens are placeholders, not working economy/combat systems.

## Entry checklist for future updates

For each meaningful increment, record its date, version/branch, actual additions, test results, known limitations, and next action. After publication, add its fixed tag/download and source checkpoint. Mark features as implemented, verified, or released only when each corresponding step has actually succeeded. Retain old entries and release checkpoints; update current status when work moves forward.
