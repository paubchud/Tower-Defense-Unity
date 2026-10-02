# Steam setup and verification

Status: version **0.1.2 experimental development playtest**, with an owner-authorized main catch-up and completed-branch cleanup. The [public GitHub prerelease ZIP](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2) is published; anonymous download and checksum verification passed. Two-account/device Steam P2P is still unverified. You do not need to register your game's App ID for this SDK test. The production App ID remains zero; stable `v0.1` remains the earlier Unity Relay build.

## Play privately with a friend now

1. Both sign into Steam on **separate accounts/devices** and be Steam friends. Download **TowerDefense-0.1.2-Windows.zip** from the [v0.1.2 prerelease Assets](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.2), not GitHub's source-code ZIP. The same ZIP is local in `Builds`.
2. Both extract the whole ZIP. Open **Start-Private-Steam-Test.cmd**. This launches the game with explicit `-td-steam-playtest` and keeps it open for normal play; it is not the SDK diagnostic that exits automatically.
3. Alternatively, open `TowerDefense.exe` in the current build folder, select Play and a class, then click **Enable Private Steam Test (480)**. Both players must enable this mode. The owner chose to remove the root shortcut.
4. The host chooses **Host Steam**, then **Copy Room Code**. Share that numeric code privately; your friend selects their class, enters the code, and chooses **Join Steam**. Both press **Ready**. The host must keep the game open.
5. Test movement, camera, hotbar/panels, reset, leave and fresh rehost with swapped classes. This remains the Step 1 foundation, not implemented combat/troop sending.

This mode uses Valve's shared [Spacewar example App ID 480](https://partner.steamgames.com/doc/sdk/api/example). Steam may display **Spacewar**, not our game name. Private mode is explicit and development-only, uses friends-only two-player lobbies and matching game/version checks, and keys test identity under `steam-test`, not production `steam`. It is not a secret/private dedicated machine hosted by Valve; your PC runs the match and Steam supplies lobby/P2P relay services.

Prefer numeric room codes for this test. Both should already have our game running when using **Invite Friend**: an accepted invite while it is closed may launch the actual Spacewar application, because we are borrowing its App ID. The overlay may be unavailable for direct launches; code-based joining is the fallback. Accepted invites update an already open connection page without choosing a class or switching an active match automatically.

A public GitHub **experimental test download** is now authorized; the launcher's "Private" name refers to the development/friends-only mode, not the ZIP's visibility. Do not represent App ID 480 as this game's production Steam/itch identity. Real two-device P2P replication and separate-home reachability still require a friend test; local checks cannot prove them. Guest internet play/login/saves are not implemented yet—see [GUEST_PLAY_PLAN.md](GUEST_PLAY_PLAN.md). Guest play will not require Steam, and cross-play need not be supported.

## Before production Steam distribution (later)

1. Sign in at [Steamworks](https://partner.steamgames.com/) and complete the developer onboarding for your own game. You handle account agreements, payment, and private identity/banking details; do not send those details or publisher keys in chat.
2. Obtain the game's numeric **App ID**. This number is not a secret; it is safe to tell the coding agent. Steam Direct currently charges **$100 USD (or equivalent) per app**, a publishing fee distinct from recurring multiplayer usage. See [Valve's fee page](https://partner.steamgames.com/doc/gettingstarted/appfee).
3. In Unity, select `Assets/TowerDefense/Resources/SteamSettings.asset` and set **App Id** to that number. Zero disables ordinary Steam online play; only explicit development test mode selects 480.
4. Give your testing Steam accounts access to the app through the appropriate development/beta/playtest setup. An App ID alone does not grant game ownership or a working Steam distribution configuration. Follow [Valve's initialization requirements](https://partner.steamgames.com/doc/sdk/api).
5. Use two separate Steam accounts on two devices for the multiplayer check. Two copies on one PC remain useful for LAN diagnostics, but they do not prove two-user Steam networking.

You do not need an Epic account/product or Unity Relay billing for this branch. Saved progression/Steam Cloud, achievements, a public store page, and ranked/trusted rewards are separate future work.

## Player flow after the App ID is configured

1. Both players sign into Steam, have access to this game's App ID, and use the same build/version.
2. Open the game, select Warrior/Wizard, and click **Host Steam**.
3. The host clicks **Invite Friend** to open Steam's invitation overlay, or shares **Copy Room Code** with a Steam friend. These are friends-only, two-member lobbies, not anonymous public matchmaking.
4. The friend chooses their class and clicks **Join Steam**. Accepted/startup lobby invites supply the numeric lobby code for selection; the player confirms joining. There is no automatic class choice or forced mid-match switch.
5. Both ready up. The host remains the authority and must keep the game open. Losing the host ends the connection; automatic host migration is not implemented.

SteamNetworkingSockets P2P routes through Steam's networking/relay infrastructure rather than a Unity Relay allocation. No router port forwarding is part of this flow; actual two-device reachability still needs verification. The overlay may be unavailable when launching outside Steam; the code-based path provides a recoverable alternative.

## Builds and publishing

Use **Tools > Tower Defense > Build Versioned Windows Player**. A completed build packages the executable, Steam native runtime, Unity runtime/data and build information, updates an existing shortcut only if present, then removes older generated local builds/ZIPs. A removed shortcut stays removed. Keep only the latest playable build locally; published older versions and source checkpoints remain on GitHub for recovery. Failed publication leaves the working previous build intact; validation logs/unrelated files are not removed.

Normal production publication refuses Steam App ID **0** or Valve's test ID **480**. With the owner's explicit authorization, `Tools/PublishBuild.ps1 -SteamTestPrerelease` can instead publish a public **experimental prerelease** with the test launcher available; it cannot mark this as the latest stable release. The development build includes the launcher/test readme and remains usable for Steam or LAN tests with zero configured. Stable `v0.1` remains intact. Publishing a GitHub ZIP is not the same as uploading Steam depots or verifying remote multiplayer. Production builds must omit the development private mode/launcher and use the real App ID.

Steam depot uploads must include the complete playable build and correctly configured executable/depots. Do not upload development-only `steam_appid.txt` files or Valve's sample identity as the game's production configuration. The runtime uses a process-local App ID hint for direct development launches; Steam ownership/configuration still has to be valid.

Steamworks.NET generates a local `steam_appid.txt` containing its sample ID when importing in Unity. This generated file is ignored by Git, and the ZIP packager now explicitly excludes it. The game's ordinary flow refuses 0/480 before initializing Steam; private mode is an explicit development override. Configure `SteamSettings` for production, not the generated sample file.

## SDK diagnostic (not a multiplayer acceptance test)

With Steam running, the development build has an explicit opt-in diagnostic:

```powershell
& '.\Builds\TowerDefense-0.1.2-Windows\TowerDefense.exe' -td-steam-private-test -screen-fullscreen 0 -logFile "$PWD\Builds\Validation\steam-private.log"
```

This uses **Valve's example App ID 480 only for the private diagnostic**, creates/leaves/rehosts the developer's own lobby, then exits. It does not invite/message friends or test a second player. It must never be packaged as this game's public Steam identity. A `TD_STEAM_PRIVATE_PASS` proves only SDK initialization, Steam identity, and lobby lifecycle—not the NGO P2P data path, production entitlement, overlay invitations, or two-device gameplay. See [Valve's example application](https://partner.steamgames.com/doc/sdk/api/example).

Use the current executable path from `Builds/latest-build.json`, not necessarily the older example path above. The unset-App-ID preview also accepts `-td-steam-config-test` instead: it verifies a recoverable ordinary-mode setup error and no native initialization. `-td-steam-playtest-check` exercises the real private-mode menu, SDK, lobby, native P2P listen/NGO host, copied code, leave and fresh rehost with swapped classes, then exits. Its pass marker does not prove a second peer. Neither diagnostic sends friend invitations/messages. Normal `-td-steam-playtest` is the playable mode and does not run these diagnostics or auto-exit.

## Required peer gate before Step 2 and production publishing

- For private networking verification, both accounts use explicit development App ID 480 mode; production publishing separately requires the real App ID and valid account entitlements.
- Host/join through both a numeric code and a Steam invite; same-version checks and invalid/full rooms behave correctly.
- Two accounts/devices synchronize class/side, movement, hotbar, readiness, reset, leave, disconnect, and fresh rehost.
- A third player cannot join, and a nonmember cannot attach a P2P connection. Cancel/retry/late callbacks must not create an unwanted arena.
- Host loss must not silently promote the client into a new authoritative match.
- Repeat on separate internet connections; check Steam unavailable/offline and overlay unavailable cases.
- Verify all runtime files in the ZIP/depot, run automated tests/LAN regression, and record evidence in `DEVELOPMENT_LOG.md`.
- The owner explicitly authorized merging the current source into main before this real-peer gate is complete; that cleanup is not evidence of multiplayer verification. Complete the checks before starting Step 2 as version 0.2. Do not publish 480 as a production release. Keep the guest-provider boundary and independent identity namespaces.
