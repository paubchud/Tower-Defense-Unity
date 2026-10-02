# Project progress record

The Unity project is in `Tower Defense PVP`. Read `README.md`, `DEVELOPMENT_PLAN.md`, and the current section of `DEVELOPMENT_LOG.md` before making project changes.

For every meaningful gameplay, networking, build, tooling, or release increment, update `DEVELOPMENT_LOG.md` in the same source increment:

- Put new dated entries first, using the owner's America/Chicago timezone.
- Record the version and branch, what actually changed, checks actually run/results, limitations, and the next required action.
- Distinguish implemented, verified, released, and planned. Never report unfinished or untested features as complete.
- Update the current-status summary and record fixed release tags/downloads after publication succeeds.
- Preserve older entries and recovery checkpoints; do not erase history to make progress appear complete.

Use `codex/` update branches, preserve existing user changes, and retain published tags/downloads. Main may be merged only when authorized and the relevant update is verified. Latest owner decision: playable private Steam development testing using Valve's App ID 480 now, with architecture prepared for guest play later. Guest identity/rooms/transport must not require Steam; separate player profile identity from per-match Netcode IDs and keep test/Steam/guest profile namespaces distinct. Cross-play is not required, and EOS remains canceled: do not install/enable a guest backend or paid service without a new selection/authorization. Private testing does not require the game's own App ID; production Steam distribution does. Keep 480 explicit, development-only and out of public releases. Verify real two-account/device replication before the networking merge; then Step 2 remains version 0.2. The published 0.1 prototype still uses Unity Relay.

Use the versioned builder for versioned Windows ZIPs and automatic updates of the existing executable shortcut. Publish tested downloadable updates with `Tools/PublishBuild.ps1`; never silently replace published releases or enable paid service billing.
