# Guest play preparation

Guest internet play is a future feature, not enabled by this increment. In source **0.1.4**, the owner chose one automatic startup sign-in flow and explicitly kept Steam/guest matchmaking separate. Steam sign-in opens the main menu automatically; unsuccessful/unconfigured Steam offers explicit local guest sign-in or retry. Local guest identity now persists with a versioned GUID/backup and does not require Steam. It is not a saved progression system or EOS authentication.

The owner reconsidered the earlier EOS cancellation and created an EOS product, Peer2Peer client, Live sandbox and deployment. EOS is now the selected guest direction, but the SDK/client secret/device login/room service/transport have not been connected. No guest internet or billing is enabled. Existing fixed Steam downloads stay unchanged. Do not confuse portal setup, local guest profile creation and verified EOS internet multiplayer.

## Boundaries already implemented

- `IOnlineProvider` owns platform-specific code validation, identity/room preparation, transport setup, invitation support, errors and room cleanup. `SteamOnlineProvider` is the current adapter. Match startup consumes an `OnlineConnection`, not a Steam ID or a specific transport.
- `AccountIdentity` is a provider-namespaced profile key. `steam`, `steam-test` and future `guest` identities cannot collide even when their subject strings match. It is not a display name, per-match Netcode client ID, filename, password or proof of authentication.
- `LocalAccount` is the current connection's local identity and clears on failed connect/leave. `SignIn.Account` is the separate startup/profile identity and survives match cleanup. Neither is server-verified remote authentication. Steam test mode never pretends to be a production Steam profile.
- Game rules, classes, inventories and server-owned actions remain shared. Provider changes are allowed only outside a connection; canceled/late results cannot switch the active match's transport or identity.
- Automated fixtures can supply a non-Steam identity, non-Steam room code and LAN transport without initializing Steam. These fixtures do not implement public guest networking.

## Implementation order when guest play is requested

1. Choose the guest distribution and networking provider before installing another SDK. Support a separate Guest connection option/pool; Steam-versus-guest cross-play is not required. Steam's sample App ID is not an anonymous guest service, and a guest must not need to log into Steam.
2. Local profile identity is implemented in 0.1.4: create a guest GUID once, atomically commit first creation, retain a backup, recover an existing valid backup and reject corrupt/unknown-version data without silent profile replacement. Progression saves/migrations remain to implement before unlocks. Clearly explain that a device-local guest profile can be lost and is not cross-device login.
3. Add a guest provider implementing `IOnlineProvider`. It must prepare its own identity, room credentials and transport, validate codes before service calls, and clean up canceled/late room operations. Do not leave the Steam SDK eagerly initialized in guest-only builds: make provider construction/SDK loading conditional for that increment.
4. Prove host/join, NAT/relay reachability without port forwarding, same-version matching, two-player/member limits, cancellation, retry, disconnect, reset and rehost on two devices/homes. Apply the same authoritative gameplay and ownership validation as Steam. Choose explicit limits; do not assume unlimited free anonymous relay traffic.
5. Add optional real account login/recovery only when needed. Guest-to-account linking requires ownership proof and a conflict/migration policy; do not silently merge currencies, duplicate rewards or import arbitrary client-supplied Steam/account IDs. Keep test data isolated.
6. Add trusted rewards/cloud persistence separately if competitive progression needs them. A local profile and a player-hosted match are not a trusted reward backend. Choose deployment, quotas and costs explicitly before enabling a public service.

## Open decisions

EOS is selected for independent guest networking; no cross-play is requested. Next: use Epic's supported SDK/plugin, provide least-privilege game-client credentials locally (never in chat/public Git), implement real EOS Connect Device ID login separately from the local profile GUID, replace `UnconfiguredGuestProvider` with a functioning room/transport adapter, and verify two-device/home host/join/cancel/reset/rehost. The initial local GUID is not an EOS Device ID and must not be treated as a credential. Optional recoverable guest saves, account linking/conflict rules, distribution/limits and trusted rewards remain open. No paid usage plan has been enabled.
