# Project progress record

The Unity project is in `Tower Defense PVP`. Read `README.md`, `DEVELOPMENT_PLAN.md`, and the current section of `DEVELOPMENT_LOG.md` before making project changes.

For every meaningful gameplay, networking, build, tooling, or release increment, update `DEVELOPMENT_LOG.md` in the same source increment:

- Put new dated entries first, using the owner's America/Chicago timezone.
- Record the version and branch, what actually changed, checks actually run/results, limitations, and the next required action.
- Distinguish implemented, verified, released, and planned. Never report unfinished or untested features as complete.
- Update the current-status summary and record fixed release tags/downloads after publication succeeds.
- Preserve older entries and recovery checkpoints; do not erase history to make progress appear complete.

Use `codex/` update branches during implementation, preserve existing user changes, and retain published tags/downloads. Latest owner cleanup request explicitly authorizes catching main up to the current 0.1.2 source and removing all other local/remote branches once their history is contained in main. This does not mean real two-account/device Steam P2P has passed; keep that limitation explicit. Future merges still need authorization, and Step 2 remains 0.2 after real peer testing. The 0.1.2 ZIP is a public experimental GitHub prerelease; keep friends-only lobbies and the guest-ready architecture. Guest identity/rooms/transport must not require Steam; separate player profile identity from per-match Netcode IDs and keep test/Steam/guest profile namespaces distinct. Cross-play is not required, and EOS remains canceled: do not install/enable a guest backend or paid service without a new selection/authorization. Development testing uses explicit App ID 480; production Steam distribution requires the game's own App ID. Public experimental downloads are allowed only with `Tools/PublishBuild.ps1 -SteamTestPrerelease`, never as stable/production releases. Published stable 0.1 still uses Unity Relay.

Keep only the newest successfully packaged local build folder and ZIP in Builds; retain validation logs and unrelated files. Cleanup must follow successful ZIP/manifest/optional existing-shortcut promotion, never follow filesystem links or delete outside Builds, and leave the previous working build intact on failed publication. The owner chose to remove the root executable shortcut; do not recreate it automatically. Game binaries remain ignored by Git and downloadable through Releases, not committed to main. Publish tested downloads with `Tools/PublishBuild.ps1`; never silently replace published releases or enable paid service billing. Recovery uses preserved Git commits/tags and GitHub downloads, not local old-build copies.
