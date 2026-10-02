# Steam setup and verification

Status: Steam code is on `codex/steam-integration`, version 0.1.1 preview. The game's App ID is not configured yet. Do not confuse this with the already published Unity Relay `v0.1` prototype.

## What you need to provide

1. Sign in at [Steamworks](https://partner.steamgames.com/) and complete the developer onboarding for your own game. You handle account agreements, payment, and private identity/banking details; do not send those details or publisher keys in chat.
2. Obtain the game's numeric **App ID**. This number is not a secret; it is safe to tell the coding agent. Steam Direct currently charges **$100 USD (or equivalent) per app**, a publishing fee distinct from recurring multiplayer usage. See [Valve's fee page](https://partner.steamgames.com/doc/gettingstarted/appfee).
3. In Unity, select `Assets/TowerDefense/Resources/SteamSettings.asset` and set **App Id** to that number. Zero deliberately disables Steam online play rather than using someone else's app.
4. Give your testing Steam accounts access to the app through the appropriate development/beta/playtest setup. An App ID alone does not grant game ownership or a working Steam distribution configuration. Follow [Valve's initialization requirements](https://partner.steamgames.com/doc/sdk/api).
5. Use two separate Steam accounts on two devices for the multiplayer check. Two copies on one PC remain useful for LAN diagnostics, but they do not prove two-user Steam networking.

You do not need an Epic account/product or Unity Relay billing for this branch. Saved progression/Steam Cloud, achievements, a public store page, and ranked/trusted rewards are separate future work.

## Player flow after the App ID is configured

1. Both players sign into Steam, have access to this game's App ID, and use the same build/version.
2. Open the game, select Warrior/Wizard, and click **Host Steam**.
3. The host clicks **Invite Steam Friend** to open Steam's invitation overlay, or shares **Copy Room Code** with a Steam friend. These are friends-only, two-member lobbies, not anonymous public matchmaking.
4. The friend chooses their class and clicks **Join Steam**. Accepted/startup lobby invites supply the numeric lobby code for selection; the player confirms joining. There is no automatic class choice or forced mid-match switch.
5. Both ready up. The host remains the authority and must keep the game open. Losing the host ends the connection; automatic host migration is not implemented.

SteamNetworkingSockets P2P routes through Steam's networking/relay infrastructure rather than a Unity Relay allocation. No router port forwarding is part of this flow; actual two-device reachability still needs verification. The overlay may be unavailable when launching outside Steam; the code-based path provides a recoverable alternative.

## Builds and publishing

Use **Tools > Tower Defense > Build Versioned Windows Player**. A completed build packages the executable, Steam native runtime, Unity runtime/data, and build information, then updates the existing repository shortcut. Earlier builds are retained.

The release publisher refuses a Steam build with App ID **0** or Valve's test ID **480**. A local preview can still be built and LAN-tested with zero configured. The public GitHub latest download remains `v0.1` until a new version passes its production App ID and multiplayer checks. Publishing a GitHub ZIP is not the same as uploading Steam depots.

Steam depot uploads must include the complete playable build and correctly configured executable/depots. Do not upload development-only `steam_appid.txt` files or Valve's sample identity as the game's production configuration. The runtime uses a process-local App ID hint for direct development launches; Steam ownership/configuration still has to be valid.

Steamworks.NET generates a local `steam_appid.txt` containing its sample ID when importing in Unity. This generated file is ignored by Git and was verified absent from the preview ZIP. The game's normal flow refuses 0/480 before initializing Steam; configure `SteamSettings`, not the generated sample file.

## Private SDK diagnostic (not a public release)

With Steam running, the development build has an explicit opt-in diagnostic:

```powershell
& '.\Builds\TowerDefense-0.1.1-Windows\TowerDefense.exe' -td-steam-private-test -screen-fullscreen 0 -logFile "$PWD\Builds\Validation\steam-private.log"
```

This uses **Valve's example App ID 480 only for the private diagnostic**, creates/leaves/rehosts the developer's own lobby, then exits. It does not invite/message friends or test a second player. It must never be packaged as this game's public Steam identity. A `TD_STEAM_PRIVATE_PASS` proves only SDK initialization, Steam identity, and lobby lifecycle—not the NGO P2P data path, production entitlement, overlay invitations, or two-device gameplay. See [Valve's example application](https://partner.steamgames.com/doc/sdk/api/example).

The unset-App-ID preview also accepts `-td-steam-config-test` instead. That diagnostic exercises the real Host Steam menu/retry, verifies no native initialization occurs, and reports `TD_STEAM_CONFIG_PASS` only when the setup error is recoverable. It makes no live Steam request. Use the current executable path from `Builds/latest-build.json` if repeated builds have a `-build2` suffix.

## Required gate before merging and publishing

- Real App ID configured; both testing Steam accounts have access.
- Host/join through both a numeric code and a Steam invite; same-version checks and invalid/full rooms behave correctly.
- Two accounts/devices synchronize class/side, movement, hotbar, readiness, reset, leave, disconnect, and fresh rehost.
- A third player cannot join, and a nonmember cannot attach a P2P connection. Cancel/retry/late callbacks must not create an unwanted arena.
- Host loss must not silently promote the client into a new authoritative match.
- Repeat on separate internet connections; check Steam unavailable/offline and overlay unavailable cases.
- Verify all runtime files in the ZIP/depot, run automated tests/LAN regression, and record evidence in `DEVELOPMENT_LOG.md`.
- Only then merge/push the verified networking update, tag/publish it, and start Step 2 as version 0.2 on its own branch.
