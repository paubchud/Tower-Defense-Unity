# Guest play preparation

Guest internet play is a future feature, not enabled by this update. Private Steam testing uses two Steam accounts now; LAN diagnostics still work without Steam. EOS remains canceled, and no guest backend or billing is being enabled.

## Boundaries already implemented

- `IOnlineProvider` owns platform-specific code validation, identity/room preparation, transport setup, invitation support, errors and room cleanup. `SteamOnlineProvider` is the current adapter. Match startup consumes an `OnlineConnection`, not a Steam ID or a specific transport.
- `AccountIdentity` is a provider-namespaced profile key. `steam`, `steam-test` and future `guest` identities cannot collide even when their subject strings match. It is not a display name, per-match Netcode client ID, filename, password or proof of authentication.
- `LocalAccount` is the current connection's local identity and clears on failed connect/leave. It is not a saved profile or a server-verified remote account. Steam test mode never pretends to be a production Steam profile.
- Game rules, classes, inventories and server-owned actions remain shared. Provider changes are allowed only outside a connection; canceled/late results cannot switch the active match's transport or identity.
- Automated fixtures can supply a non-Steam identity, non-Steam room code and LAN transport without initializing Steam. These fixtures do not implement public guest networking.

## Implementation order when guest play is requested

1. Choose the guest distribution and networking provider before installing another SDK. Support a separate Guest connection option/pool; Steam-versus-guest cross-play is not required. Steam's sample App ID is not an anonymous guest service, and a guest must not need to log into Steam.
2. Add versioned local profiles before persistent unlocks. Create a guest GUID once and persist it with atomic writes, backups and migrations; do not regenerate it each launch, match or connection. Use safe encoded filenames or JSON keys rather than raw colon-containing profile keys. Clearly explain that a device-local guest profile can be lost and is not cross-device login.
3. Add a guest provider implementing `IOnlineProvider`. It must prepare its own identity, room credentials and transport, validate codes before service calls, and clean up canceled/late room operations. Do not leave the Steam SDK eagerly initialized in guest-only builds: make provider construction/SDK loading conditional for that increment.
4. Prove host/join, NAT/relay reachability without port forwarding, same-version matching, two-player/member limits, cancellation, retry, disconnect, reset and rehost on two devices/homes. Apply the same authoritative gameplay and ownership validation as Steam. Choose explicit limits; do not assume unlimited free anonymous relay traffic.
5. Add optional real account login/recovery only when needed. Guest-to-account linking requires ownership proof and a conflict/migration policy; do not silently merge currencies, duplicate rewards or import arbitrary client-supplied Steam/account IDs. Keep test data isolated.
6. Add trusted rewards/cloud persistence separately if competitive progression needs them. A local profile and a player-hosted match are not a trusted reward backend. Choose deployment, quotas and costs explicitly before enabling a public service.

## Open decisions

Guest networking/backend, local-only versus recoverable guest saves, account linking/conflict rules, public-test distribution and limits. No paid usage plan or cross-store networking guarantee has been selected. Preserve the Steam adapter and reusable match rules while implementing the chosen guest adapter.
