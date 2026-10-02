# Project progress record

The Unity project is in `Tower Defense PVP`. Read `README.md`, `DEVELOPMENT_PLAN.md`, and the current section of `DEVELOPMENT_LOG.md` before making project changes.

For every meaningful gameplay, networking, build, tooling, or release increment, update `DEVELOPMENT_LOG.md` in the same source increment:

- Put new dated entries first, using the owner's America/Chicago timezone.
- Record the version and branch, what actually changed, checks actually run/results, limitations, and the next required action.
- Distinguish implemented, verified, released, and planned. Never report unfinished or untested features as complete.
- Update the current-status summary and record fixed release tags/downloads after publication succeeds.
- Preserve older entries and recovery checkpoints; do not erase history to make progress appear complete.

Use `codex/` update branches, preserve existing user changes, and retain published tags/downloads. Main may be merged only when authorized and the relevant update is verified. The owner requested EOS guest multiplayer first, then merge/push that verified result, then Step 2 as version 0.2. EOS developer setup is currently pending; the 0.1 prototype still uses Unity Relay.

Use the versioned builder for versioned Windows ZIPs and automatic updates of the existing executable shortcut. Publish tested downloadable updates with `Tools/PublishBuild.ps1`; never silently replace published releases or enable paid service billing.
