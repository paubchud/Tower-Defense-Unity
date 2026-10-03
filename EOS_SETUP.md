# EOS guest internet play - 0.1.5 experimental source

Steam and guests use one startup/menu flow but **separate matchmaking pools**. Direct launch with no valid configured Steam session offers Play as Guest. The game uses EOS Connect Device ID, not an Epic account sign-in, Steam login or the profile's local GUID as a credential. The GUID stays a device-local profile key; the authenticated EOS Product User ID has the separate `eos-guest` namespace. No cloud progression, account linking/recovery or competitive reward service is included.

## Owner configuration

The existing Epic product, Live sandbox/deployment and **TowerDefense-Guest / Peer2Peer** game client are the selected configuration. Use the least-privilege game-client policy for Connect, Lobbies and P2P. Do not grant trusted-server/admin privileges, disable user authentication as a shortcut, enable billing, or use administrator/publisher credentials. A Device ID guest is still an authenticated EOS Connect user.

The repository-root **`.local/eos-settings.json`** is ignored by Git. Schema:

```json
{
  "schemaVersion": 1,
  "productId": "YOUR_PRODUCT_ID",
  "sandboxId": "YOUR_SANDBOX_ID",
  "deploymentId": "YOUR_DEPLOYMENT_ID",
  "clientId": "YOUR_LIMITED_GAME_CLIENT_ID",
  "clientSecret": "YOUR_LIMITED_GAME_CLIENT_SECRET",
  "forceRelay": true
}
```

Enter the secret locally; never paste it in chat, commit it, print the JSON in logs, or put trusted-server keys in the game. The versioned builder copies only this validated game-client configuration into the player **after** Unity's build succeeds, outside Assets, before packaging/promotion. A missing/invalid configuration prevents promotion and keeps the previous packaged game intact. Runtime reads the packaged copy; players do not enter credentials themselves. Changing the local JSON requires a new build for friends.

**A downloadable client cannot keep its embedded game-client secret confidential.** It can be extracted from the game. Its safety depends on least privilege and authenticated-user service policy, not obfuscation or Git ignore rules. Do not distribute a privileged credential. Published downloads/older source tags are never replaced silently.

## Source dependency setup

Before opening a fresh source checkout, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\InstallEosSdk.ps1
```

This verifies SHA256 `aafe5a1cc278f2f65e0373777706d0a1028eea49b9548ef4a7520cc50f647c4b` for the pinned upstream **EOS-Contrib plugin 6.2.0** archive (source `65e44063d5f908002e74a3080343fb0be561dacb`). It installs only the unmodified Epic C# SDK and Windows x64/Universal Mac native EOS libraries plus licenses in an ignored local UPM package. The full plugin's sample build tooling, overlay, native renderer and integrated-Steam features are deliberately not installed. This avoids its incompatible Steamworks.NET editor helper and unnecessary Visual Studio/configuration requirements. Game Steam integration remains independently pinned and unchanged. Native SDK files are not committed to public Git. There is no new paid service or account UI.

## Guest play / friend test

1. Both players use the complete **same-version** Windows/Mac package. Current 0.1.5 local packages are experimental source outputs, not automatically a new GitHub release. Older 0.1.3 downloads cannot use this guest adapter.
2. Launch the executable/app directly and choose **Play as Guest**. Do not use the explicit Steam test launcher for a guest test. Watch the EOS status at the menu; a configuration/service error does not erase the local guest profile. Retry Host Guest after correcting connectivity; a packaged configuration fix needs a rebuild.
3. Play, select a class, **Host Guest**, copy/share the **full TDG room code**, then the other guest selects **Join Guest**. Both ready up. The host must stay open. One device's EOS Device ID cannot connect to itself; a localhost three-process LAN pass is not a guest internet peer pass.
4. Test separate devices and home networks, both host roles, movement/selection replication, third-member rejection, reset/readiness, leaving/canceling, disconnected host, and fresh rehost/rejoin. With `forceRelay=true`, all guest peer traffic requests EOS relay rather than requiring user port forwarding; actual reachability/bandwidth/latency still must be measured on real networks.

Guest rooms are code-selected **EOS public-advertised lobbies**, with an unguessable full lobby ID and exact game/version/pool/format metadata validation before joining. They are **not password-protected private/invite-only rooms**, and can be queried through EOS. No matchmaking browser/queue is exposed. Steam lobbies remain friends-only. Lobby membership controls accepted guest P2P peers; packet/socket IDs are scoped to each room. A host leaving ends the room: no silent authority migration.

EOS packet frames are at most 1170 bytes. Reliable NGO messages are reassembled with a 64 KiB message ceiling, at most eight pending assemblies per peer, bounded native queues and a fragment deadline; oversized unreliable messages or failed reliable sends end the connection instead of pretending delivery succeeded. These are safety ceilings, not a proven eight-player performance budget.

Run `Tools/ValidateEosGuest.ps1 -Capture` for a **single-device/service** diagnostic: real Device ID/Connect login, canceled create/late-room destruction, NGO host, leave and fresh rehost. It sends no invitations/messages and never deletes the EOS Device ID. This is not proof of two-peer P2P, Windows/Mac execution or different-home reachability. Validation logs stay in Builds/Validation; review/redact EOS IDs and room codes before sharing logs.

## Future queue and parties

Core contracts define a distinct private-room/queue entry, provider pool, version, immutable party/leader roster and data-driven team count/size. Party membership cannot duplicate players, exceed one team's size, or mix Steam/Steam-test/guest pools. Per-match team-seat reservation is atomic and keeps whole parties on one team; it is used for today's solo 1v1 admission. The future `four-team-duos` format describes **four teams of two / eight players** but is deliberately not playable.

Before enabling queues: implement authenticated party invites/acceptance/leader changes/disband/reconnect separately from match-room lifecycle; verify whole-party consent and admission; add search/ticket/cancel/timeout/assignment ownership, version/pool/region/skill policy and a fair host-selection/concurrent-admission strategy. Leaving a match must not automatically disband a party. Party snapshots are data, not proof of membership/authentication.

Before enabling 2v2v2v2: supply a four-team map, team spawn/lane/castle ownership and ally/enemy targeting, ready/results rules, full eight-member services/transport admission, disconnect policy and measured server/CPU/bandwidth budgets. Current cameras/UI/combat/map are not already eight-player ready. No queue matching algorithm, persistent/networked party, ranked fairness, cloud rewards, dedicated server or host migration is implemented by these contracts.

Primary references: [EOS Connect](https://dev.epicgames.com/docs/epic-online-services/eos-fundamentals/connect-interface), [pinned SDK package](https://github.com/EOS-Contrib/eos_plugin_for_unity/releases/tag/v6.2.0).
