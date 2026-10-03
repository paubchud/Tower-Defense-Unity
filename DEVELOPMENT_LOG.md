# Tower Defense PVP - Development Log

This is the progress record: what has actually been added, what has been tested, and what is still unfinished. Dates use the owner's America/Chicago timezone. New updates go at the top; older entries remain available.

The implementation order lives in [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md), the intended gameplay in [GAME_DESIGN.md](GAME_DESIGN.md), and play/download instructions in [README.md](README.md). A planned feature is not a completed feature.

## Current status

- **Step 1 / version 0.1:** released on GitHub as fixed tag `v0.1`, source checkpoint `881ee16`. [Download/release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1). Its Windows ZIP passed size/SHA256 verification and standalone regression.
- **Local builds/launcher:** keep only the newest successful build folder and ZIP per platform (Windows and macOS), plus validation logs. The owner chose to leave the root shortcut removed; the builder respects that choice rather than recreating it. Old release downloads remain on GitHub.
- **GitHub recovery:** each published version will have a fixed `v<version>` tag and retained downloadable ZIP. Work stays on separate update branches; commits do not erase earlier snapshots.
- **Current networking work:** experimental Steam 0.1.2 is now in `main`, with explicit playable App ID 480 development mode and guest-ready provider/identity boundaries. All 51 current automated tests passed; prior Windows packaging, native single-host/leave/rehost and LAN checks remain recorded. Real two-account/device Steam P2P verification is pending. A real App ID is a production-distribution requirement, not a blocker for development tests. Released 0.1 still uses Unity Relay.
- **Public playtest download:** [v0.1.2 experimental prerelease](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) remains at fixed source `a340482`. Its existing ZIP passed LAN, GitHub size/SHA256/tag and full anonymous-download checks. Stable `v0.1` and friends-only lobby behavior are retained; the release-log link now points to main.
- **Source cleanup:** owner-authorized main catch-up is committed/pushed at `27e77c9`; four remote and five local extra branches were deleted only after their ancestry was verified in main. New implementation is on `codex/macos-playtest`; main remains 0.1.2 until a new merge is authorized. Binaries stay ignored; ZIPs stay in Releases. No commits, tags or release assets were deleted; the earlier merge does not verify remote Steam P2P.
- **Windows/Mac 0.1.3:** [public experimental prerelease](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3) published at fixed source `c70dd13`, on `codex/macos-playtest`. Both complete anonymous downloads passed size/SHA256 verification. 55/55 EditMode tests, both builds, Mac static package validation, Windows LAN/controls and single-host Steam/configuration checks passed. Mac launch, security, rendering/input/native Steam and real two-account Windows/Mac P2P remain unverified. Main is unchanged; see [MAC_TESTING.md](MAC_TESTING.md).
- **Guest play:** future requirement restored. Provider/identity separation is implemented; guest internet/login/persistent saves are not. No backend/billing is enabled; EOS remains canceled. See [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md).
- **Next gameplay release:** 0.2, Step 2's combat and troop sending, after real two-account/device Steam networking verification. Current source is already in main by the owner's explicit cleanup request.

## 2026-10-03 - Windows/Mac compatibility update 0.1.3

Status: implemented and locally verified compatibility/build tooling; **paired public experimental prerelease published**. Automated, both builds, Windows standalone and Mac static package checks passed. Both full anonymous downloads passed integrity checks. Not a verified Mac runtime or remote Steam peer release.
Version/branch: `0.1.3`, `codex/macos-playtest`, from clean main `b101681`. The owner authorized Mac build support and a new paired Windows/Mac experimental GitHub download, with only the latest local output per platform. No new main merge is authorized.
Fixed tag/source: [`v0.1.3` / `c70dd13`](https://github.com/paubchud/Tower-Defense-Unity/commit/c70dd1396d5765072567dea9072f7af99e22f8f4). [Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3) / [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.3/TowerDefense-0.1.3-Windows.zip) / [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.3/TowerDefense-0.1.3-macOS.zip). The tooling-fix commit changes no player code or ZIP from the `5986590` implementation.

### Changed

- Installed Unity 6000.6.3f1's official Mac Build Support (Mono). The Hub headless follower reported the module was already selected after the original installer completed; the actual MacStandaloneSupport engine is present and the build is using it.
- Added a Universal Intel/Apple Silicon Mono Mac build command. Kept gameplay, friends-only Steam lobbies, explicit App ID 480 mode and guest-neutral identity/provider boundaries unchanged; no EOS, guest backend or billing was enabled.
- Platform-specific build folders and manifests retain one latest successfully packaged local Windows and Mac folder/ZIP independently. Existing Windows entry points remain compatible; failed publication protects previous working output. Removed root shortcut stays removed.
- Mac packaging checks bundle version and Universal player/Unity/Steam binaries, records POSIX executable modes and Unix creator metadata in its ZIP, excludes sample App ID/debug backup files and refuses filesystem links/ZIP64. Added all-file ZIP/hash/architecture/permission validation and a Mac launcher/instructions with honest unsigned/unnotarized and untested-runtime limitations.
- Paired publisher requires matching version/provider/test-mode metadata, validates Mac statically and Windows via LAN, then verifies both uploaded ZIPs before publishing one experimental prerelease. Fixed older tags/downloads and stable v0.1 remain unchanged; no source or binaries are merged into main by the publisher.

### Checks / next required action

- 55/55 EditMode tests passed, 0 failed/skipped (`Builds/Validation/macos-playtest-tests.xml`), including independent platform retention, failed Mac publication protection, Universal architecture rejection and Unix ZIP modes/creator metadata. Windows PowerShell 5.1 syntax checks passed for the publisher and Mac validator.
- Both versioned builds completed with `TD_PUBLISH_PASS`/`TD_BUILD_PASS`: `Tower Defense PVP/Logs/macos-0.1.3-build.log` and `windows-0.1.3-build.log`. Mac static validation passed under Windows PowerShell 5.1: correct bundle version, Universal Intel/Apple Silicon main/Unity/Steam binaries, every packaged file hash, all Unix creator metadata and executable modes. No Mac execution is implied.
- Exact Windows 0.1.3 LAN host/client/third-player checks passed, including input, panels, replication, reset, disconnect and swapped-class rejoin (`Builds/Validation/Smoke-20261003-004742`). Windows client arena and private-host lobby captures were visually reviewed. Native Steam playable-menu/SDK/lobby/NGO host/P2P-listen/code-copy/leave/rehost passed; ordinary unset-App-ID guard/retry passed with no native initialization (`Builds/Validation/Steam-0.1.3/playable.log`, `config.log`). These are single-account Steam checks, not a real peer pass.
- Paired publisher dry-run passed; mixed Windows 0.1.2/Mac 0.1.3 was correctly rejected before authentication. Normal production publication still rejects App ID 0/480. PowerShell syntax and `git diff --check` passed; no generated binaries are tracked.
- Windows ZIP: 71,694,299 bytes; SHA256 `e41259575252f127421bb96f6c2f1b9a18043f536b058ec44151a3a066c3aa9f`. Mac ZIP: 110,941,339 bytes; SHA256 `511588e780cf44298025f58ad01a6c074e88996cd6436bb5a330cc5a8dbee669`. Both ZIPs include runtime/test instructions and exclude sample App ID/debug backups. GitHub upload metadata and complete anonymous streamed downloads match these exact lengths/hashes; no duplicate local archives were saved.
- Local Builds now keeps only the 0.1.3 Windows folder/ZIP and Mac folder/ZIP, their manifests and Validation. Successful Windows packaging removed the old local 0.1.2 folder/ZIP after promotion; its fixed GitHub download remains recoverable. No root shortcut was recreated. Unity's temporary performance-test Resources files were automatically removed by its build postprocessor; no generated test-run metadata is committed.
- Source implementation was committed/pushed at `5986590`. The publisher repeated exact-build LAN checks successfully (`Builds/Validation/Smoke-20261003-005053`), then stalled before draft creation: Windows PowerShell 5.1 serialized `Get-Content`'s attached PSDrive/PSProvider metadata in the nested notes body. Stopped only our owned publisher process. An authenticated read-only GitHub audit confirmed **no v0.1.3 draft/release was created**. Fixed notes loading to plain `File.ReadAllText`; the reproduction now produces a JSON string body in ~0.002 seconds instead of a metadata object. No player code/ZIP changed; publication retry follows the tooling-fix commit.
- Corrected publisher retry from pushed `c70dd13` passed Mac static validation and another exact-build Windows LAN run (`Builds/Validation/Smoke-20261003-005554`). It streamed/verified both ZIPs, then published `v0.1.3` with `draft=false`, `prerelease=true`, not latest stable. Independent unauthenticated API/full-download audits passed for both assets and fixed tag at `c70dd1396d5765072567dea9072f7af99e22f8f4`. Fetched the new tag locally for recovery. Original `v0.1` (`881ee16`), `v0.1.2` (`a340482`), old preview asset digest, latest stable `v0.1`, and main `b101681` remain unchanged. No releases/assets were replaced, no merge/force-push occurred.
- No real Mac is available on this Windows host. Friend must verify Mac launch/Gatekeeper, rendering/input/performance, Steam native loading and two-account/device Windows/Mac replication on separate networks. No Developer ID signing/notarization was performed. These checks remain required even after experimental publication; Step 2 stays 0.2 after real-peer verification.

## 2026-10-02 - Latest local build only and main catch-up

Status: local cleanup, automated verification, main catch-up/push and extra-branch removal complete. Game remains 0.1.2; no player runtime change, new release or replacement ZIP.
Source checkpoint: [`27e77c9`](https://github.com/paubchud/Tower-Defense-Unity/commit/27e77c9), committed on temporary `codex/latest-build-only` from `709d40f`, then fast-forwarded/pushed into main; the temporary branch is now removed. The owner explicitly asked to catch main up and delete/unactivate remaining branches, chose source in main / ZIP in Releases, and chose to leave the root shortcut removed. This supersedes the earlier local old-build/always-shortcut policy and authorizes the current main merge despite real Steam peer testing still pending.

### Changed

- The versioned builder removes older generated build folders/ZIPs only after successful ZIP/manifest/optional existing-launcher promotion. Unknown files and Validation logs remain; cleanup checks immediate absolute targets and refuses filesystem links. Locked/unremovable older output logs a warning instead of invalidating the new build.
- Failed publication retains the previous working build. A deleted/missing root shortcut is not recreated.
- Recycled exactly five folders: `Prototype`, `InternetPrototype`, `TowerDefense-0.1-Windows`, `TowerDefense-0.1.1-Windows`, `TowerDefense-0.1.1-Windows-build2`; and their three versioned old ZIPs plus `TowerDefense-Internet-20261001.zip`. They can be restored from Windows Recycle Bin. Latest 0.1.2, manifest and Validation remain untouched.
- Updated launch/recovery/retention instructions. Preserved fixed tags, published ZIPs and commit history rather than keeping old local builds. Updated the public prerelease's development-log link to main; uploaded ZIP and fixed release tag were not changed.
- Main advanced without a rewrite from `079e2d2` to `27e77c9`, preserving the complete source chain. Verified all branch tips are ancestors before deleting remote `codex/internet-join-codes`, `codex/steam-integration`, `codex/private-steam-playtest`, `codex/public-playtest-download` and their local refs plus the local cleanup branch. Local/remote inventory now shows only main (and its origin tracking/HEAD alias).

### Verified / remaining

- 51/51 EditMode tests passed, 0 failed/skipped (`Builds/Validation/latest-build-only-tests-final.xml`), including new successful cleanup, failed-publication protection, unpublished/external-target rejection and removed-shortcut tests.
- The first restricted Unity runner could not reach licensing IPC; stopped only that owned stalled process, then retried with access to the installed licensing service. Successful runner log: `Tower Defense PVP/Logs/codex-latest-build-only-tests-retry.log`.
- Builds now contains only `TowerDefense-0.1.2-Windows`, its ZIP, `latest-build.json` and `Validation`. ZIP SHA256 remains `22ebbfc0ebfcfbab353c9317bd20a6a3197e7bc67368e85fd3cfd221787e7939`; root shortcut remains absent by request. No player rebuild was needed for editor-tooling-only changes.
- Post-push containment checks and `git ls-remote --heads origin` passed: only main remains remotely; local inventory agrees. Fixed tags `v0.1` and `v0.1.2` remain. No force-push, reset, release replacement or commit/history deletion was used.
- Real two-account/device Steam P2P remains unverified. Next gameplay milestone is still 0.2 after those checks; no guest backend or billing changed.

## 2026-10-01 - Public Steam playtest download 0.1.2

Status: published public **experimental prerelease**, not production Steam distribution or verified two-user P2P. The owner explicitly requested a public download, superseding the earlier private-only ZIP distribution decision.
Branch: `codex/public-playtest-download`, based on `cdf1cf1`. This changes release tooling/instructions, not gameplay or the existing executable/shortcut. Main and older downloads are unchanged.
Fixed tag/source: [`v0.1.2` / `a340482`](https://github.com/paubchud/Tower-Defense-Unity/commit/a340482). [Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) / [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.2/TowerDefense-0.1.2-Windows.zip). No existing release/tag/asset was replaced.

### Changed

- Added an explicit `-SteamTestPrerelease` publisher option for an experimental App ID 480 development ZIP. Normal production App ID guards remain. This route requires the test launcher, publishes as a prerelease, never promotes it to latest stable and cannot replace an existing release or change main.
- Added versioned download/launch instructions and limitations in `Releases/0.1.2.md`, including separate Steam accounts/devices, complete extraction, Spacewar identity, friends-only lobbies and unverified remote P2P.
- Updated repository/setup/plan instructions for public ZIP distribution without committing generated Builds or giving friends source write access. Future guest play remains unimplemented and no backend/billing changed.
- The packaged private readme predates the owner's public-download decision; release notes clarify its outdated "not public distribution" line without replacing the published/tested ZIP. Runtime test-mode limitations remain unchanged.

### Verification / remaining

- Reusing the exact locally verified 0.1.2 ZIP: 71,694,212 bytes; SHA256 `22ebbfc0ebfcfbab353c9317bd20a6a3197e7bc67368e85fd3cfd221787e7939`. Its 47 automated tests and native single-host checks are recorded below; no runtime code changed here.
- Publisher PowerShell syntax passed; default production publication rejected the Steam preview, while explicit `-SteamTestPrerelease -DryRun` selected the correct ZIP without authentication/upload. ZIP contents passed the executable/runtime/Steam DLL/launcher/readme checks and exclude local `steam_appid.txt`, Unity backup folders, `.git` and `.env` files. Size/SHA256 match the previously verified package.
- The publisher repeated LAN regression against the exact executable: host/client/third PASS, including controls/reset/disconnect/swapped-class rejoin (`Builds/Validation/Smoke-20261001-233918`). It then streamed the ZIP, checked GitHub's asset size/SHA256 and published as `prerelease=true`, `draft=false`, without promoting it to latest stable.
- Independent **unauthenticated** GitHub checks passed: public prerelease metadata/ZIP digest, fixed tag at `a34048292d485b40bbce69d0dcee11f2385a55e8`, unchanged main `079e2d2` and latest stable `v0.1`. Downloaded the complete ZIP anonymously as a stream; length and computed SHA256 match the original package. Friends do not need GitHub access invitations.
- No Unity runtime change/rebuild or new automated/native Steam test was needed for this distribution-only update; previously recorded 47 tests/single-host checks remain the runtime evidence.
- Real two-account/device Steam P2P and separate-home reachability remain unverified; public availability must not imply those checks passed. Do not merge networking yet; Step 2 remains 0.2 after that gate.

## 2026-10-01 - Private Steam playtest 0.1.2 and guest preparation

Status: implemented and locally verified; private ZIP ready for a friend test. Not a production Steam release or verified two-user internet match.
Branch: `codex/private-steam-playtest`, based on the pushed 0.1.1 checkpoint; main and published tags/downloads remain unchanged.
Source checkpoint: [`7de363e`](https://github.com/paubchud/Tower-Defense-Unity/commit/7de363e), committed and pushed. The ZIP is a local private development artifact, not a public GitHub/Steam release.

### Added

- Explicit development-only private Steam mode using Valve's sample App ID 480, enabled from the connection menu or `-td-steam-playtest`. Normal play still does not silently select the sample app.
- Playable private launcher/readme in development ZIPs; it keeps the normal menu/host/join game open, unlike the SDK-only diagnostic that exits. The same repository shortcut remains tied to the latest completed versioned build.
- Private-test menu/lobby labels, room-code fallback and accepted-invite updates to an open connection page. Invite controls are hidden for LAN/unsupported providers.
- Provider-neutral connection preparation/transport/code validation/error/cleanup boundary, with the current Steam adapter and provider switching restricted outside a connection.
- Namespaced account identity separate from per-match Netcode IDs. Production Steam, Steam-test and future guest profile keys cannot alias; local connection identity clears on failure/leave. No save/profile/backend is implemented by this type.
- Explicit ZIP exclusion of `steam_appid.txt`, in addition to Unity's do-not-ship backup exclusion. Public Steam publication guards for App ID 0/480 remain unchanged.
- Guest implementation sequencing, stable local GUID/profile migrations/recovery, optional linking/conflict checks, independent guest transport and separate player pools in [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md). No cross-play requirement, paid service or EOS integration was added.
- Updated setup/plan/repository instructions: private SDK testing does not require registering the game, while production distribution does; guest preparation must not force Steam into future match rules.

### Verified so far

- 47/47 automated tests passed (`Builds/Validation/private-steam-tests.xml`), including private/production launch policy, account namespace validation, provider switching restrictions, a non-Steam identity/code/LAN transport host without Steam initialization, existing foundation/lifecycle/packet checks and launcher/archive safeguards.
- Tests use fake providers/local networking, not a second Steam account or guest internet backend.
- Final source recheck: 47/47 passed, 0 failed/skipped (`Builds/Validation/private-steam-tests-final.xml`).
- Actual 0.1.2 Windows build/package succeeded. ZIP contains the game, Steam native runtime, `Start-Private-Steam-Test.cmd`, private test readme and manifest; explicit `steam_appid.txt`/Unity backup exclusions passed. The shortcut targets this executable with its build directory as working directory.
- ZIP SHA256: `22ebbfc0ebfcfbab353c9317bd20a6a3197e7bc67368e85fd3cfd221787e7939`. This private development ZIP is not a public release asset.
- Real private-menu check passed on this developer account: App ID 480 activation, native SDK/lobby/P2P listen, NGO playable host, copied room code, leave, fresh rehost as Wizard and cleanup (`Builds/Validation/Private-Steam-0.1.2/playable-host.log`). It sent no invitations/messages and used no second peer.
- Ordinary-mode unset-App-ID/retry check also passed without native initialization (`unset-config.log` in that folder). Production publication dry run rejected the private/unconfigured preview before authentication/upload.
- Standalone LAN host/client/third checks all passed: controls/replication, third-player rejection, reset, disconnect and swapped-class rejoin (`Builds/Validation/Smoke-20261001-230106`). Private connection/lobby captures were visually checked.

### Remaining

- Real Steam two-account/device replication, invites and separate-home reachability still need a friend test. Do not merge or advertise them verified based on a one-account host check.
- No guest internet/login/save functionality, production App ID/entitlements/depot upload, Cloud/progression, matchmaking queue, trusted rewards or host migration.
- Private App ID 480 ZIPs are shared privately, not published as production game releases. After peer verification, the authorized networking merge comes before gameplay 0.2.

## 2026-10-01 - Steam integration preview 0.1.1

Status: implemented; automated tests passed; not a released or fully verified Steam multiplayer update.
Branch: `codex/steam-integration`. Based on the fixed `v0.1` checkpoint; main remains unchanged.
Source checkpoint: [`79c68ca`](https://github.com/paubchud/Tower-Defense-Unity/commit/79c68ca), committed and pushed to GitHub. This is a preview source checkpoint, not a merged or published Steam release.

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
