# Tower Defense PVP - Development Log

This is the progress record: what has actually been added, what has been tested, and what is still unfinished. Dates use the owner's America/Chicago timezone. New updates go at the top; older entries remain available.

The implementation order lives in [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md), the intended gameplay in [GAME_DESIGN.md](GAME_DESIGN.md), and play/download instructions in [README.md](README.md). A planned feature is not a completed feature.

## Current status

- **0.2.1 publication authorized/in progress:** owner requests GitHub Releases for verified playable updates, including the existing matching 0.2.1 packages. Release work continues on `codex/milestone-2-combat`; its required source-branch push is authorized, repository remains public, and main stays at `a008f71` until a completed-milestone transition. Public 0.2.1 download success is not yet claimed. Older releases/tags and stable v0.1 will be preserved.

- **0.2.1 local combat verified:** `codex/milestone-2-combat`. Host-owned Raider sends/XP upgrade/kill gold, starter towers, sword/staff attacks and energy, castle result and ghost/respawn are implemented. Final **110/110 EditMode tests**, Windows two-process combat/controls/ghost/results/reset/rejoin plus third-client rejection, both packages and Mac static checks passed. Ghost/results diagnostics use explicit development-only fixtures, not final-balance tests. No 0.2.1 GitHub release/main merge; real 0.2.1 Steam/EOS peer combat, Mac runtime, latency/loss and sustained-load measurement remain open. Version policy is `0.<milestone>.<progress>`: 0.2.1 then 0.2.2; Milestone 3 starts 0.3.1.
- **Milestone transition complete:** owner-authorized main fast-forward/push succeeded at `a008f71`, preserving completed 0.1.5/design work. Repository remains public; no existing release/tag was changed. New unfinished combat remains on its local branch. Owner reported basic Windows Steam/EOS and Mac/Windows Steam friend play; EOS Mac/Windows remains unverified.
- **Still planned:** gold spending/land/harvesting in Milestone 3, fog before invasion in Milestone 4 (ghosts grant no vision), prediction/audits/host recovery, queue/party/team gameplay, persistent progression. Current disconnect resets the lobby; host loss ends the session.

### Historical checkpoint summaries

- **Owner-reported friend play / transition checkpoint:** on 2026-10-03 the owner reported successful Windows Steam/EOS play and Mac/Windows Steam play. EOS Mac/Windows remains untested; exact versions and detailed acceptance cases were not supplied. Milestone 2 local combat work is now starting, target 0.2. Owner authorized merging/pushing the completed current branch into public main when moving milestones, superseding the source-push pause; this does not authorize replacing existing releases/tags or enabling paid services.

- **Local fog-of-war milestone plan:** `codex/multiplayer-flow-design`, game version unchanged at 0.1.5. Milestone 4 now starts with visibility-filtered fog before invasion; owner confirmed ghosts grant no vision. Milestone 2 establishes life-state vision eligibility and Milestones 2-3 separate private/public data. Prediction/compact validated actions/audits and automatic replacement-host recovery remain planned; full authoritative checkpoints cannot be disclosed to all ordinary clients without leaking fog, and a modified player host can inspect authority state. Documentation only, no runtime/build/release or GitHub push.

- **0.1.5 EOS guest update:** [published experimental Windows/Mac release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5), fixed source `855f4af` on `codex/eos-guest-networking`. Both complete anonymous downloads passed size/SHA256 checks. Actual EOS Connect Device ID/TDG rooms/independent bounded NGO P2P-relay adapter are connected. Prior 92/92 tests, Windows single-device EOS/Steam/startup/LAN and Mac static checks passed; publication repeated exact-build LAN and Mac static validation. No main merge; real guest/Steam peer reachability, Mac runtime and saved progression remain unverified/unimplemented. Self-connect error/role-aware cleanup remains a disclosed issue; no billing enabled.

- **Local multiplayer design/privacy update:** on `codex/multiplayer-flow-design`, [MULTIPLAYER_FLOW.md](MULTIPLAYER_FLOW.md) and GAME_DESIGN.md specify solo/party 1v1/2v2v2v2/4v4 queues, pre-queue builds, all-ready/loading, reconnection and premade/random-filled leaving rules. Design only, not shipped queue/reconnect/team gameplay. Owner confirmed full-premade classification and chose to keep GitHub public while stopping further source pushes. Local changes are not uploaded; existing public source/history remains visible. No visibility change, deletion/history rewrite or new repo occurred.

- **0.1.4 unified startup source:** implemented and locally verified on `codex/unified-sign-in`, based on clean 0.1.3 branch checkpoint `1d67bf9`. Automatic valid Steam sign-in or explicit guest fallback before the main menu is implemented, with stable versioned local guest identity/backup. 75/75 tests, both local packages, Windows startup/LAN/single-account Steam checks and Mac static validation passed. Separate Steam/guest matchmaking was explicitly confirmed; EOS portal setup exists and supersedes the prior cancellation, but SDK/device login/rooms/transport and progression saves are not connected. Guest internet actions are visibly disabled. No 0.1.4 release or main merge has occurred; Mac execution and real two-account Steam peer checks remain open.

- **Step 1 / version 0.1:** released on GitHub as fixed tag `v0.1`, source checkpoint `881ee16`. [Download/release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1). Its Windows ZIP passed size/SHA256 verification and standalone regression.
- **Local builds/launcher:** keep only the newest successful build folder and ZIP per platform (Windows and macOS), plus validation logs. The owner chose to leave the root shortcut removed; the builder respects that choice rather than recreating it. Old release downloads remain on GitHub.
- **GitHub recovery:** each published version will have a fixed `v<version>` tag and retained downloadable ZIP. Work stays on separate update branches; commits do not erase earlier snapshots.
- **Current networking work:** experimental Steam 0.1.2 is now in `main`, with explicit playable App ID 480 development mode and guest-ready provider/identity boundaries. All 51 current automated tests passed; prior Windows packaging, native single-host/leave/rehost and LAN checks remain recorded. Real two-account/device Steam P2P verification is pending. A real App ID is a production-distribution requirement, not a blocker for development tests. Released 0.1 still uses Unity Relay.
- **Public playtest download:** [v0.1.2 experimental prerelease](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) remains at fixed source `a340482`. Its existing ZIP passed LAN, GitHub size/SHA256/tag and full anonymous-download checks. Stable `v0.1` and friends-only lobby behavior are retained; the release-log link now points to main.
- **Source cleanup:** owner-authorized main catch-up is committed/pushed at `27e77c9`; four remote and five local extra branches were deleted only after their ancestry was verified in main. New implementation is on `codex/macos-playtest`; main remains 0.1.2 until a new merge is authorized. Binaries stay ignored; ZIPs stay in Releases. No commits, tags or release assets were deleted; the earlier merge does not verify remote Steam P2P.
- **Windows/Mac 0.1.3:** [public experimental prerelease](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3) published at fixed source `c70dd13`, on `codex/macos-playtest`. Both complete anonymous downloads passed size/SHA256 verification. 55/55 EditMode tests, both builds, Mac static package validation, Windows LAN/controls and single-host Steam/configuration checks passed. Mac launch, security, rendering/input/native Steam and real two-account Windows/Mac P2P remain unverified. Main is unchanged; see [MAC_TESTING.md](MAC_TESTING.md).
- **Guest play:** published 0.1.5 includes EOS guest internet login/rooms/transport independently of Steam, using the selected portal configuration and limited game-client credential. Real guest peer connectivity and progression saves are not verified/implemented. Published 0.1.3 predates guest networking. See [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md).
- **Next gameplay release:** 0.2, Step 2's combat and troop sending, after real two-account/device networking verification. Only the prior 0.1.2 source was caught up to main by the owner's cleanup request; 0.1.3/0.1.4/0.1.5 remain on update branches pending new merge authorization.

## 2026-10-03 - Release-on-playable-update policy and 0.2.1 publication preparation

Version **0.2.1**, branch **codex/milestone-2-combat**, gameplay checkpoint **d15d8ca**. Owner now asks that changes be published to Releases for testing. Recorded a standing policy to publish verified playable increments, not unfinished experiments or documentation-only edits; authorize the normal publisher's tested source-branch push to the still-public repository. Main merge policy remains completed milestone transitions. No gameplay/source behavior or build contents changed in this release-preparation increment.

Existing packages: matching Windows/Mac 0.2.1 build2, with prior 110/110 tests and Windows combat/ghost/results/controls/reset/rejoin/rejection plus Mac static verification recorded below. Updated release notes to clearly describe an experimental Steam + EOS guest playtest and its incomplete scope. Next: dry-run paired artifact selection/static checks, commit/push the release checkpoint, repeat exact-build regression, publish with `Tools/PublishBuild.ps1 -SteamTestPrerelease -IncludeMacOS`, verify fixed tag/public metadata and anonymous full ZIP downloads, then record actual results/links. This entry does not claim a new publication, real internet combat, Mac runtime or a main merge. Preserve prior downloads/tags; no billing or credential disclosure is authorized.

## 2026-10-03 - 0.2.1 first combat slice (locally verified, not released)

Version **0.2.1**, branch **codex/milestone-2-combat**, based on main transition checkpoint **a008f71**. Owner changed version numbering to `0.<milestone>.<progress>`; retain older published tags. Main catch-up was fast-forwarded/pushed successfully before starting this increment.

Implemented in source: authority-only pure combat model and editable rules; stable per-match seat IDs independent of Netcode IDs; round/sequence duplicate protection; bounded sends and host-only diagnostic transaction journal; XP/upgrade/gold transactions; own-lane sword/staff attacks with class energy costs; class-group starter towers; castle victory/draw/results; ghost state with no attack/collection/vision eligibility, retained management/economy and home respawn. Ghost sends default off pending owner choice. Gold spending, build/harvest and fog are not implemented. Private energy/economy use owner-read replication; troop list deltas and local time-derived pooled visuals avoid per-frame troop transform traffic. Death/respawn affects authority movement immediately; match simulation bounds catch-up work after stalls.

Checks actually run:

- Initial and final Unity EditMode runs: **110/110 passed** (18 new combat tests), including duplicate/stale/invalid commands, cooldown/cap, future-only atomic upgrade, range/aim/weapon/energy and multi-pool atomic rejection, exactly-once defender gold, tower targeting, ghost restrictions/management/respawn, castle outcome/draw, end rejection, new-round identity and bounded journal. Final results: `Builds/Validation/combat-0.2.1-tests-final.xml`; final log: `Tower Defense PVP/Logs/combat-0.2.1-tests-final.log`. Active editor target restored to Windows.
- Windows and Universal Mono Mac builds/package promotion passed at 0.2.1. Final local paths are `Builds/TowerDefense-0.2.1-Windows-build2` and `Builds/TowerDefense-0.2.1-macOS-build2`, with their ZIPs and separate latest manifests. The `build2` suffix is a rebuild of this same increment, not a new progress version. Only these newest successful generated folders/ZIPs remain; older generated local copies were cleaned after successful promotion, with validation/unrelated files retained. Existing GitHub recovery downloads/tags remain untouched; root shortcut stays absent.
- First LAN combat run in `Smoke-20261003-165445`: combat send/XP/energy/private replication passed, then the wheel diagnostic failed because the preceding combat test selected slot 0 while the existing wheel test expected slot 1. Corrected the test setup to explicitly select slot 1; this was not represented as a game pass.
- Final `Tools/ValidatePrototype.ps1 -Combat -Capture` in `Builds/Validation/Smoke-20261003-170034`: **host, client and rejected third client passed**. Both real named-input controls and combat RPC/list replication passed; the remote client had no opponent private energy/XP state. A development-only low-health/teleport fixture exercised real troop-hit death, ghost movement and rejected attack, no vision eligibility, timed home respawn and retained XP; a near-castle fixture exercised arrival/victory/result replication and post-result rejection. Reset clears combat/list/economy/round state; leave/rejoin swaps classes without stale state. These fixtures are opt-in diagnostic code, not production abilities or final gameplay-balance/stress tests.
- `Tools/ValidateMacBuild.ps1`: **TD_MAC_PACKAGE_PASS v0.2.1**, including Universal player/native Steam/EOS libraries, bundle version, every ZIP file hash and Unix executable modes. Static only: unsigned/unnotarized Mac launch/security/input and actual 0.2.1 Mac peer play remain unverified.
- Live combat, ghost and victory captures visually checked: HUD, tower/troop geometry, panels, pale ghost and result/reset UI are readable. `git diff --check` passed; binaries/local EOS settings remain Git-ignored. No credential values were copied into source/log documentation or chat.

Next: friend-test matching 0.2.1 builds over Steam/EOS in both roles; finish EOS Mac/Windows and measured delay/loss/load/balance checks before calling the Milestone 2 gate complete. Prior owner feedback does not verify this new code. GitHub still publishes fixed experimental 0.1.5; 0.2.1 is local, not uploaded/released. No prediction, full fog filtering, periodic audit service, host migration, queue service, authenticated reconnect, persistent saves, gold spending/land/harvesting or paid billing was added. A ghost's zero vision eligibility is groundwork; current clients still receive public enemy transforms/stats until Milestone 4 filtering.

## 2026-10-03 - Owner peer feedback and milestone-transition checkpoint

Status: owner-reported basic peer play and authorized source checkpoint; not a new runtime/build/release or an independently observed full platform acceptance pass. Existing source version 0.1.5; checkpoint branch `codex/multiplayer-flow-design`, followed by planned Milestone 2 branch `codex/milestone-2-combat` targeting 0.2.

- Owner reports playing with a friend using both Steam and EOS on Windows and Steam between Mac/Windows. Record those as basic owner-reported passes only. EOS between Mac/Windows is explicitly still unverified; no test logs, exact versions, both-host-role/reset/invite matrix or latency/loss measurements were supplied. Existing automation/static package checks are separate evidence.
- Owner requests beginning Milestone 2 and permits current completed branches to merge/push to main at each new milestone. This explicitly supersedes the stop-further-source-pushes decision; repository remains public and uploading includes source. Preserve all fixed tags/downloads and existing history; no release replacement, branch cleanup or paid service is authorized by this request.
- Current fog/design edits were preserved and documentation instructions updated before the transition. Runtime combat scope is troop sends/upgrades, hero/group-tower attacks and energy, XP/gold, castle victory/reset and timed ghost respawn with zero ghost vision. Queues, full fog, prediction, reconnect and replacement-host recovery remain distinct later/network increments.
- Checkpoint verification/merge/push will be recorded after success. No new Unity runtime test is claimed by this documentation checkpoint. Next: branch for local Milestone 2, choose documented editable prototype values, implement and verify the combat slice, and retain EOS Mac/Windows as a required follow-up.

## 2026-10-03 - Fog-of-war milestone and no ghost scouting

Status: local documentation increment only; scheduled feature, not implemented/tested gameplay. Version: existing 0.1.5 unchanged; next gameplay milestone remains 0.2. Branch: `codex/multiplayer-flow-design`, continuing the local design checkpoint; no main merge or remote push.

- Owner requested enemy locations/stats be sent only while visible and explicitly prohibited ghost scouting from revealing fog, then delegated milestone placement. Assigned full fog delivery to the beginning of **Milestone 4, before invasion**, preserving existing milestone numbering. Milestone 2 establishes ghost/living vision eligibility; Milestones 2-3 keep private authority/economy state separate from opponent-visible payloads.
- Updated GAME_DESIGN.md's fog specification, ghost rules and prototype scope; DEVELOPMENT_PLAN.md's milestone table, implementation sequence, visibility acceptance gate and performance/privacy guardrails; MULTIPLAYER_FLOW.md's reconnect/replacement-host boundaries; README.md and AGENTS.md's current instructions. Ghost camera/movement never creates vision; independent authorized vision and permitted own-side management remain separate.
- Required conceal/reveal snapshots and filtering across transforms/stats, action-ledger/events, UI/minimap, cues and reconnect, not only a screen overlay. Radius/occlusion, territory-wide intruder detection, eligible other revealers/team sharing, visible stats, last-seen and blind-attack rules remain open instead of being invented as confirmed decisions.
- Recorded the owner's preferred replacement-host direction and responsiveness/compact-action requirements as planned work. Immediate authoritative validation remains necessary; periodic audits do not replace it. Readable full-match backups leak hidden data, and a player host needs full simulation state; observer filtering/ledger hashes do not provide cheat-proof host secrecy. Recovery/privacy design and real-peer latency/loss tests remain gates, with no paid service selected.
- Checks: six-file Markdown-only scope, all 30 local document links, milestone-first visibility sequence, no-ghost-scouting acceptance, reconnect/host privacy boundaries and unchanged next gameplay version passed (`DOC_CHECK_PASS`). Exact comparison confirmed older dated development-log history is unchanged. `git diff --check` passed; Git's LF-to-CRLF notices are line-ending warnings, not whitespace errors. No Unity runtime tests/builds were run and no new fog/prediction/migration test pass is claimed. Existing published tags/downloads, source privacy pause and local builds remain untouched.
- Next: verify real guest/Steam peers and Mac, authorize/measure the responsiveness increment, then implement Milestone 2 with these boundaries. At Milestone 4 choose remaining vision rules, pass client-payload/no-ghost-reveal tests before adding invasion, and resolve checkpoint trust/privacy before enabling host migration.

## 2026-10-03 - Multiplayer flow design and source-push pause

Status: local documentation increment only; no new runtime/build/release or Milestone 2 implementation. Branch: `codex/multiplayer-flow-design`, locally branched from fixed published 0.1.5 source `855f4af`. Game version remains 0.1.5; next gameplay milestone remains 0.2.

- Before Milestone 2, owner requested a League-like solo/friend/team queue with class/build selected before entering, CSGO-style individual ready-up staging and reconnection. Added MULTIPLAYER_FLOW.md and integrated it into GAME_DESIGN.md/DEVELOPMENT_PLAN.md and guest/setup docs.
- Defined party sizes for solo 1v1, solo/duo 2v2v2v2 and parties up to four in 4v4; immutable build/party/mode consent, separate provider/version pools, whole-party packing, atomic assignment, readiness/loading acknowledgements and results/requeue. Today's runtime still only enables 1v1; 4v4 was not added to its catalog.
- Recorded exact 60-second 1v1 rejoin/otherwise-opponent-win requirement. Owner confirmed an entirely premade pre-queue team has no compensation buff or extra leaver penalty, but can still lose. Mixed/random-filled teams get a reconnect opportunity, then abandoned-player removal and a modest balance buff. Team grace duration, buff target/values/cap, precise leaver consequence and double-abandonment rules remain explicit TBDs, not invented final rules.
- Specified stable authenticated match-player/team/seat ownership independent of new transport IDs, separate life/disconnect states, bounded admission/deadlines, state/transaction/result recovery and anti-abuse testing. Original party classification cannot change to evade policy. Flagged host quitting/crashing as an unsolved authority-survival gate; EOS/Steam relay does not keep the player-hosted game running. No paid infrastructure is authorized.
- Owner privacy choice: keep the repository public for now and stop further source pushes. No pushes occurred after that decision, including these documents/log updates. Existing public source/history and the already published 0.1.5 remain visible. No remote visibility change, source deletion, force-push, new repository or release replacement. The new design branch is local only.
- Updated local current-status/download instructions for successfully published 0.1.5 and retained historical release entries. Document cross-reference checks, all-three-mode/1v1-deadline consistency, credential exclusion and `git diff --check` passed. Changed files are Markdown only; no runtime change requires a new Unity build/test run. Prior 92/92 runtime test evidence is not reported as new queue/reconnect verification. Local recovery commit is not pushed.
- Next: review unresolved policy/authority details before those implementations, verify real guest/Steam peers and Mac, then choose Milestone 2 combat/energy/economy defaults. Any further GitHub source push requires new authorization.

## 2026-10-03 - Publish EOS guest playtest 0.1.5

Status: owner-authorized experimental publication succeeded and both complete anonymous downloads passed verification. Version/branch: `0.1.5`, `codex/eos-guest-networking`, using unchanged verified Windows/Mac packages from the implementation entry below. Fixed tag/source: [`v0.1.5` / `855f4af`](https://github.com/paubchud/Tower-Defense-Unity/commit/855f4af83c0f8efed0c0d07a1919829b3cccca4b). No gameplay/runtime fix or main merge. This publication/source push preceded the owner's later stop-further-pushes decision.

- Owner asked to make the EOS version a new GitHub release. Publish paired experimental downloads through `Tools/PublishBuild.ps1 -SteamTestPrerelease -IncludeMacOS`, preserving old releases/tags and latest stable v0.1. Local ZIPs include the extractable limited EOS game-client credential, not a server/admin key; source credentials remain ignored. No billing changes.
- Added release-note instructions for same-PC LAN testing and the reported guest self-connect/start-failure/`LobbyNotOwner` cleanup warning. Read-only inspection of the owner's Player.log showed successful EOS authentication and client room entry, then transport startup failure and attempted destroy cleanup. Code rejects matching local/host EOS identity but chooses destroy/leave by identity rather than connection role. This supports the same-account explanation and identifies a known cleanup/error-reporting follow-up; neither is fixed by publication.
- Pre-publication unauthenticated GitHub audit confirmed existing v0.1/v0.1.2/v0.1.3 release IDs, sizes/digests and tags; no v0.1.5 exists. Remote main remains `b101681`; latest stable is v0.1. Prior runtime evidence remains 92/92 tests, Windows live EOS/Steam single-host/startup/LAN checks and Mac static checks, not two-device guest/Steam or real Mac runtime verification.
- Paired Mac static validation, publisher dry run, credential exclusion scan and source whitespace checks passed. Prepared/disclosed notes were committed/pushed at `855f4af`. The publisher repeated exact-build Windows LAN host/client/third-player controls/reset/disconnect/rejoin checks successfully (`Builds/Validation/Smoke-20261003-150436`), then uploaded both assets and checked their size/SHA256 before publishing `draft=false`, `prerelease=true`, `make_latest=false`.
- [Release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5), release ID `402671436`. [Windows ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.5/TowerDefense-0.1.5-Windows.zip): **80,355,745 bytes**, SHA256 `f81e7d5d819e962c870607d36c3ce6c78ca0ac97fabe38890b8425eb79a99f8d`. [Mac ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.5/TowerDefense-0.1.5-macOS.zip): **129,999,271 bytes**, SHA256 `95d789922a4a84168b84b98f70cf224774b25abba7a72a0c0c1fa88f9146cf03`. Mac's local `-build2` ZIP uses the canonical published platform name; bytes are unchanged.
- Independent unauthenticated API and full streamed-download audits passed for both ZIPs: exact bytes/hash, correct public experimental flag and fixed tag at `855f4af`. No duplicate archives were saved. Confirmed old release IDs/assets/digests/tags v0.1/v0.1.2/v0.1.3, main `b101681` and latest stable v0.1 unchanged. Fetched new fixed v0.1.5 locally for recovery. Binaries/credentials remain ignored in source; root shortcut remains absent.
- Release notes disclose same-PC EOS identity failure/cleanup warning and LAN workaround, extractable limited game-client credential, separate Steam/guest pools, unsigned/unnotarized Mac, and unverified real peers/Mac execution. Publication does not verify those runtime gates or implement queue/party/reconnect. Next: separate-device/home guest/Steam tests, both host roles, real Mac launch/security/input and a separately authorized cleanup/error-reporting fix.

## 2026-10-03 - EOS guest networking and future queue/party foundations 0.1.5

Status: implemented and locally packaged; final automated/single-device EOS/Steam service, Windows startup/LAN and Mac static checks passed. Not a published release, main merge, real remote guest/Steam peer or Mac runtime pass.
Version/branch: `0.1.5`, `codex/eos-guest-networking`, based on pushed 0.1.4 checkpoint `9a700fe`. Owner authorized EOS implementation and later queue/party/2v2v2v2 preparation; guest and Steam pools stay separate. Publication preference was asked separately; no new merge is authorized.
Source recovery checkpoint: [`afbd402`](https://github.com/paubchud/Tower-Defense-Unity/commit/afbd402c08c23516b537df7e91510fe4cb61e3c1), committed and pushed to [`codex/eos-guest-networking`](https://github.com/paubchud/Tower-Defense-Unity/tree/codex/eos-guest-networking). This is source recovery, not a new GitHub Release download. Post-push remote audit confirms main remains `b101681`, fixed `v0.1`/`v0.1.2`/`v0.1.3` tags are unchanged and no `v0.1.5` tag exists. Published 0.1.3 remains the public download until separate approval/publication succeeds.

### Changed

- Real EOS Connect Device ID/create-user/login and authentication-expiry refresh, without Epic/Steam account UI. Local GUID/profile identity remains separate from authenticated `eos-guest` Product User ID and match-only transport IDs; no saves/recovery/linking/trusted rewards are implied.
- TDG code validation, two-member EOS code-selected advertised lobbies, exact game/version/guest-pool/duel/private-entry metadata validation, no automatic host migration, member-gated peer acceptance and room-scoped sockets. These rooms are not password-protected private/invite-only rooms; Steam lobbies remain friends-only. Failed/canceled/late requests are bounded and clean up acquired rooms before applying state.
- Custom NGO EOS transport using relay by default; native 1170-byte packet ceiling, explicit unreliable sequencing, bounded 64 KiB reliable fragmentation/reassembly, queue/assembly/time limits and fail-closed reliable-send errors. No latency or eight-player load guarantee follows from these safety bounds.
- Owner entered limited Peer2Peer game-client credentials only in ignored `.local/eos-settings.json`; they were not printed in chat/source/logs. Builder copies validated game-client config into the build after Unity succeeds, plus SDK notices/setup instructions. Downloaded clients contain an extractable game-client credential, never an administrator/trusted-server key. No billing or portal mutation occurred.
- Checksum-pinned SDK installer (`Tools/InstallEosSdk.ps1`) installs only official C# SDK/unmodified Windows x64/Universal Mac native libraries from upstream plugin 6.2.0 into an ignored local UPM package. A full-plugin trial exposed obsolete Steamworks constants and sample build configuration/Visual Studio prerequisites; the final dependency excludes those editor/sample/overlay/integrated-Steam tools. Archive SHA256 is recorded in EOS_SETUP.md. Earlier import/build failure logs remain; failed build never promoted over 0.1.4.
- Data-driven duel/four-team-duos format, distinct private/queue request, provider pools, immutable leader/party snapshots and atomic whole-party same-team seat reservations. The current 1v1 uses the allocator for solo admission. Four teams of two/eight players is defined but gated off; no live queue/party lifecycle/service/UX, four-team map/rules or eight-player network/performance verification exists.
- Guest status/menu/host/join replaces the old unavailable adapter when configured; missing configuration is explicitly disabled, errors stay retryable and do not erase the profile. Steam's explicit test mode and LAN controls remain. Added single-device service diagnostic and future party/packet tests. Publication tool now uses canonical version/platform download names and records paired EOS availability; existing release assets/tags are never replaced.

### Checks / limitations / next action

- Final matching Windows and Mac builds succeeded (`Tower Defense PVP/Logs/eos-windows-package-verified.log`, `eos-macos-package-verified.log`). Final source recheck passed **92/92, 0 failed** (`Builds/Validation/eos-source-tests-verified.xml`, `Tower Defense PVP/Logs/eos-source-tests-verified.log`), after earlier 90/90 and 92/92 iterations. Unity's active build target was restored to Windows.
- Real Windows guest diagnostic passed against the final executable in `Builds/Validation/EOS-20261003-142835`: Device ID/Connect login, cancel before native room-create callback and late destruction, NGO host, leave/destruction and fresh Wizard rehost. Native room cleanup returned success. No invitations/messages/second peer or Device ID deletion. The secret was validated locally without printing it.
- Final Windows LAN host/client/third-player controls, movement/hotbar replication, ready/reset/disconnect/swapped-class rejoin passed (`Builds/Validation/Smoke-20261003-142847`). Guest self-connect is deliberately rejected: multiple local processes share the EOS device identity and are not two guest accounts.
- Final Windows Steam regression passed automatic startup, SDK/lobby/NGO host/P2P listening, code copying, leave/fresh-rehost and retained profile with the existing developer account. Normal App ID 0 startup/retry/explicit guest consent/local identity/EOS provider checks passed without Steam native initialization (`Builds/Validation/EOS-0.1.5-Steam-final/steam.log`, `startup.log`). This is not a two-account Steam peer/invite pass.
- Final Mac package passed static Universal player/Unity/Steam/EOS native architecture, bundle/version, every ZIP file hash and Unix executable-mode checks (`Tools/ValidateMacBuild.ps1`). No Mac launch/Gatekeeper/native input or Windows/Mac peer pass; unsigned/unnotarized remains explicit.
- First hidden-window EOS screenshots were black because ordinary capture depends on presented frames. Reused the existing explicit render-to-texture capture for live scene/UI, rebuilt both platforms, and visually reviewed final `guest-eos-main-menu.png` and `guest-eos-room.png` in `EOS-20261003-142835`: successful EOS status, version, room code/ready controls and arena/HUD render correctly. Earlier black captures are retained as diagnostic history, not a visual pass.
- Final Windows folder/ZIP: `Builds/TowerDefense-0.1.5-Windows`, `TowerDefense-0.1.5-Windows.zip`; **80,355,745 bytes**, SHA256 `f81e7d5d819e962c870607d36c3ce6c78ca0ac97fabe38890b8425eb79a99f8d`. Final Mac folder/ZIP: `Builds/TowerDefense-0.1.5-macOS-build2`, `TowerDefense-0.1.5-macOS-build2.zip`; **129,999,271 bytes**, SHA256 `95d789922a4a84168b84b98f70cf224774b25abba7a72a0c0c1fa88f9146cf03`. Both latest manifests report 0.1.5/EOS configured; complete packages include the limited game-client configuration, native SDK, notices and instructions.
- Builds retains only these newest successful folder/ZIP pairs per platform, manifests and validation logs. Older local packages were removed only after successful promotion; fixed GitHub releases/commits remain recoverable. The root shortcut stays absent. Publication approval remains separate and pending; main and old fixed tags/downloads remain unchanged.
- Final credential checks passed for all Git candidate text and verification logs without printing the secret. Both ZIPs contain exactly one configuration matching the local selection, required EOS native library and notices/setup files; no local JSON, build or SDK binary is tracked. PowerShell syntax, `git diff --check` and paired experimental publisher dry run passed. The dry run used no authentication/upload or GitHub mutation. Removed only Unity's generated empty Resources-folder metadata; preserved validation history and owner configuration.
- Next required external gate: friend-test the same 0.1.5 Windows/Mac packages on separate devices/homes, both guest host roles, relay reachability/replication/latency, third-player rejection, reset/leave/rejoin, real Mac launch/security/input and independent Steam peers. Implement queue/party lifecycle/admission/fairness and a four-team map/performance gates only at a later milestone; Step 2 remains 0.2 after its real peer gate.

## 2026-10-03 - Unified startup sign-in 0.1.4

Status: implemented and locally verified startup/profile increment; Windows/Mac local packages are ready. Not a published download, EOS guest internet implementation, Mac runtime pass or real two-account Steam peer pass.
Version/branch: `0.1.4`, `codex/unified-sign-in`, from clean `1d67bf9` on the published Mac compatibility branch. Main and fixed tags/downloads are untouched; no merge/release is authorized by this startup request.
Source recovery checkpoint: [`69229c0`](https://github.com/paubchud/Tower-Defense-Unity/commit/69229c0), committed and pushed to [`codex/unified-sign-in`](https://github.com/paubchud/Tower-Defense-Unity/tree/codex/unified-sign-in). This is source recovery only; local 0.1.4 ZIPs are not GitHub Release downloads. Published 0.1.3 remains the fixed public download.

### Changed

- Added a startup state machine/service: try the configured Steam SDK identity before the main menu, enter automatically on success, otherwise show Play as Guest / Retry Steam. Development App ID 480 still requires explicit test mode; normal startup never silently chooses it. Persona display names do not become profile keys.
- Added a stable local guest GUID with schema version, atomic first write, backup fallback and rejection of unreadable/unknown-version profiles without silent replacement. Startup identity survives match leave/reset; transient Netcode connection identity remains separate. This saves identity only, not currency/equipment/unlocks or cloud data.
- Guest entry selects an explicit unavailable online adapter, not Steam or fake EOS login. Guest internet host/join stays visibly disabled and explains required setup; LAN remains usable. Steam and guest matchmaking stay separate by owner clarification.
- Updated startup/private-test diagnostics and added regression tests for automatic Steam flow, guest consent, retries/late results, namespaces, profile persistence/corruption/version/oversize handling and the unavailable guest boundary.
- Updated current instructions to reflect the owner's new EOS selection/portal setup; no EOS SDK, credential, billing or online service has been enabled. Earlier cancellation and published release entries remain historical records.

### Checks / limitations / next required action

- **75/75 Unity EditMode tests passed**, 0 failed/skipped, in both the initial and final runs (`Builds/Validation/unified-sign-in-tests.xml`, `unified-sign-in-tests-final.xml`). The 20 additional cases cover startup/profile behavior; all previous foundation/network/build tests still pass.
- Both versioned local builds/package promotions passed `TD_BUILD_PASS` and `TD_PUBLISH_PASS`; final logs are `Tower Defense PVP/Logs/unified-sign-in-windows-build-final.log` and `unified-sign-in-macos-build-final.log`. The final folders/ZIPs use the `-build2` suffix to avoid overwriting a previously working package. Bundled launch/Mac instructions match the new startup screen. The editor target was restored to Windows.
- Exact final Windows guest check passed: unconfigured App ID 0 does not initialize the native Steam SDK; explicit guest consent/retry opens the menu with a local profile, with guest internet host/join disabled (`Builds/Validation/SignIn-0.1.4-final-guest/guest.log`). The first final-check launcher accidentally concatenated a single test flag with its argument list; it did not activate the diagnostic. Stopped only that owned hidden test process, corrected the launcher and reran successfully without changing game code. Retained all prior validation logs.
- Exact final Windows automatic Steam check passed with explicit `-td-steam-playtest`: startup sign-in, menu, private lobby/NGO host/native P2P listen, code copy, leave and fresh rehost, plus unchanged startup profile after match cleanup (`Builds/Validation/SignIn-0.1.4-final/steam.log`). It used only the existing developer account and sent no invitations; it is not a two-peer pass. Initial guest/private-menu checks also passed in `SignIn-0.1.4`.
- Exact final Windows LAN host/client/third-player checks passed, including startup guest entry, controls/hotbar/panels, replication, reset, disconnect and swapped-class rejoin (`Builds/Validation/Smoke-20261003-020842`; earlier pass `Smoke-20261003-015739`). Guest-choice, guest-main-menu and automatic-Steam-main-menu captures were visually reviewed.
- `Tools/ValidateMacBuild.ps1` passed for the final Universal Mono package: Intel/Apple Silicon player/Unity/Steam libraries, bundle version, every ZIP file hash, Unix executable modes/creator metadata. Static checks do not prove Mac startup, storage access, input, native Steam or Gatekeeper behavior. The package remains unsigned/unnotarized.
- Final local Windows ZIP: 71,700,205 bytes; SHA256 `9dde5974031a7dce9d20ddb5b8d69c42c3d28bfdfa9b118469d48c14efe0a0b7`. Final local Mac ZIP: 110,947,533 bytes; SHA256 `833c064512772a2dbd7e503aed70810d622628eb049a60dbec3620df31a8a65f`. Only the latest successful folder/ZIP per platform, manifests and Validation remain in Builds. The old local 0.1.3 packages/intermediate 0.1.4 packages were removed only after successful promotion; fixed public 0.1.3 downloads remain recoverable. Root shortcut stays absent, and no binaries are tracked in Git.
- `git diff --check` passed. Main remains `b101681`; fixed `v0.1` / `881ee16`, `v0.1.2` / `a340482`, `v0.1.3` / `c70dd13` are unchanged. No release/tag replacement, branch deletion, main merge, billing or EOS portal mutation was performed.
- EOS guest internet still requires supported SDK integration, local least-privilege client credentials, Connect Device ID login, rooms/Netcode transport and separate-device/home acceptance checks. A local profile GUID is not an EOS credential. Steam two-account/device and real Mac gates remain open; no production App ID/signing/recovery/cloud rewards are added.
- Next: connect real EOS guest authentication/rooms/transport using locally configured least-privilege credentials, then test separate devices/networks and Mac runtime. Keep published 0.1.3 and main unchanged until a new release/merge is explicitly authorized. Restart the game to retry Steam after entering a guest profile; never silently switch profile identity during a session.

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
