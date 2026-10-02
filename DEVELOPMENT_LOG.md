# Tower Defense PVP - Development Log

This is the progress record: what has actually been added, what has been tested, and what is still unfinished. Dates use the owner's America/Chicago timezone. New updates go at the top; older entries remain available.

The implementation order lives in [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md), the intended gameplay in [GAME_DESIGN.md](GAME_DESIGN.md), and play/download instructions in [README.md](README.md). A planned feature is not a completed feature.

## Current status

- **Step 1 / version 0.1:** implemented and tested; versioned Windows build and ZIP created. Latest changes passed 33 automated tests and standalone multiplayer regression. GitHub release publication is in progress below.
- **Local launcher:** the existing `TowerDefense.exe.lnk` is now updated automatically by the versioned build command, after successful packaging. Older builds are retained.
- **GitHub recovery:** each published version will have a fixed `v<version>` tag and retained downloadable ZIP. Work stays on separate update branches; commits do not erase earlier snapshots.
- **Next networking update:** Steam-only compatibility and multiplayer. EOS was explicitly canceled by the owner. Steamworks App ID confirmation and SDK/transport integration are pending; current 0.1 online play still uses Unity Relay.
- **Next gameplay release:** 0.2, Step 2's combat and troop sending, after the Steam networking increment is verified, merged, and pushed.

## 2026-10-01 - Direction changed to Steam only

Status: confirmed design change; integration not yet implemented.

- The owner canceled EOS and requested Steam-only compatibility. No EOS SDK was installed, no Epic product was created, and no EOS game code needs to be removed.
- Plan Steam account identity and Steam lobby/P2P relay networking; retain local LAN diagnostics. Guest itch play and cross-store multiplayer are no longer requirements for the next networking increment.
- Preserve version 0.1's source/build checkpoint before starting Steam on a separate branch. Do not label Steam networking verified until its SDK, App ID, and multiplayer checks are actually ready.
- The release helper's first run consumed excessive memory and was stopped. No GitHub release/draft was created. HTTP requests/uploads now use bounded streaming and visible stages; retry/verification are pending.

## 2026-10-01 - Version 0.1 packaging and release workflow

Status: implemented locally; verification/publication in progress.
Branch: `codex/internet-join-codes`.

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

### Still to finish

- Commit/push the source and publish/verify the GitHub `v0.1` download.
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
