# Mac experimental playtest

**Published 0.1.5:** download [TowerDefense-0.1.5-macOS.zip](https://github.com/paubchud/Tower-Defense-Unity/releases/download/v0.1.5/TowerDefense-0.1.5-macOS.zip) from the [experimental release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.5), extract the complete ZIP in Finder and launch TowerDefense.app directly. Choose Play as Guest for anonymous EOS login, then Play/class/Host Guest or Join Guest with a full TDG code and matching Windows/Mac 0.1.5 packages on separate devices. Steam remains a separate pool using the explicit launcher. Complete anonymous download/SHA256 and static Universal native checks passed, but not real Mac launch/native loading/remote guest connectivity. Mac remains unsigned/unnotarized; follow the security notice below. Queue/party/reconnect/team modes are design/foundations only. See EOS_SETUP.md and MULTIPLAYER_FLOW.md.

**New source/local packages 0.1.4:** startup now checks Steam before the main menu. `Start-Private-Steam-Test.command` explicitly opts into App ID 480, then a valid Steam sign-in enters the menu automatically. When launching the app directly without an own-game App ID, use **Enable Private Steam Test (480)** on the startup screen. Alternatively **Play as Guest** creates/loads a local device profile and enters the menu; guest internet rooms are not connected yet. Steam/guest matchmaking stays separate. These new local packages are not a published release or a verified Mac runtime. The fixed public **0.1.3** download and its older menu instructions below remain unchanged; never mix 0.1.3/0.1.4 peers.

This is a **Windows-cross-built, Universal Mono development player** for Intel and Apple Silicon, targeting macOS 12 or newer with Metal support. Packaging checks are not a real Mac playtest. Launching, rendering, input, native Steam/EOS loading and Windows/Mac peer play still need your friend's Mac test. No paid Apple developer account or paid backend billing is configured.

## Older 0.1.3 download and open (recovery only)

1. Download **TowerDefense-0.1.3-macOS.zip** from the [0.1.3 experimental release](https://github.com/paubchud/Tower-Defense-Unity/releases/tag/v0.1.3), not the source-code ZIP. Extract the whole archive in Finder; keep its contents together. It includes `TowerDefense.app`, the Steam test launcher and instructions.
2. Open Steam and sign in. Open **TowerDefense.app**. Choose Play, select Warrior or Wizard, and choose **Enable Private Steam Test (480)**. `Start-Private-Steam-Test.command` is an optional launcher for the same mode.
3. Both friends need **0.1.3**, separate Steam accounts and devices. Windows uses the matching Windows ZIP, not an older 0.1.2 build. Host Steam, copy/share the numeric room code privately, Join Steam, then both Ready. The host's computer runs the match. Steam can show **Spacewar** because this is Valve's shared development App ID 480; it is not production Steam distribution.

## macOS security notice

This prototype is **not Developer ID signed or notarized**. macOS may block it. Review the repository, release and download before trusting it. If macOS offers a per-app **Open Anyway** approval in System Settings > Privacy & Security, use that only if you trust this particular test download. Do not disable Gatekeeper globally or remove quarantine recursively. If macOS reports damage, malware, an invalid signature, or offers no approval, stop and send the exact message; do not force it through.

ZIP entries record Unix executable permissions for the app and launcher. If your extraction tool loses those modes, extract again using Finder. If the app still will not start, report it rather than applying unreviewed terminal fixes. A Windows machine cannot verify Mac execution or perform Apple's normal signing/notarization workflow. Trusted public Mac distribution will need a later Mac signing/notarization pass; this is only an experimental friend test.

## Friend test checklist

- Record Mac model/chip, macOS version, game version and any security message. Confirm menu/text/3D map render, WASD, right-drag orbit, mouse wheel and B/U/T/I/Esc work.
- Check Steam initialization, host/code/join/ready with a Windows friend, correct classes/sides, independent controls and replicated movement/item selection. Swap host roles. Try leave, rehost, rejoin and rejecting a third player; test different home networks if possible.
- Note crashes, frame-rate problems and missing native libraries. The log is normally `~/Library/Logs/DefaultCompany/Tower Defense PVP/Player.log`. Review it before sharing and redact Steam IDs, room codes and other private information.
- No combat, sending, economy, towers, ghost respawn or persistent progression is implemented yet. Guest online play is future work; LAN diagnostics remain independent of Steam. A successful menu or single host does not establish two-user internet reachability.

Build reference: [Unity macOS build instructions](https://docs.unity3d.com/6000.6/Documentation/Manual/macos-building.html). Trusted distribution reference: [Apple Developer ID](https://developer.apple.com/developer-id/).
