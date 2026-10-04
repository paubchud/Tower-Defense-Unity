using System;
using System.Collections;
using TowerDefense.Core;
using TowerDefense.Networking;
using TowerDefense.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.Diagnostics
{
    // Explicit command-line diagnostics in development builds, never activated in normal play.
    public sealed class SmokeTestDriver : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static bool active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDiagnostics() => active = false;

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            bool host = Array.IndexOf(args, "-td-smoke-host") >= 0;
            bool client = Array.IndexOf(args, "-td-smoke-client") >= 0;
            bool reject = Array.IndexOf(args, "-td-expect-reject") >= 0;
            bool online = Array.IndexOf(args, "-td-steam") >= 0;
            if (!host && !client && Array.IndexOf(args, "-td-steam-playtest-check") >= 0)
            {
                if (active) { Destroy(gameObject); yield break; }
                active = true; DontDestroyOnLoad(gameObject);
                yield return CheckSteamPlayableMode(args); yield break;
            }
            if (!host && !client && Array.IndexOf(args, "-td-steam-config-test") >= 0)
            { yield return CheckSteamConfiguration(args); yield break; }
            if (!host && !client && Array.IndexOf(args, "-td-steam-private-test") >= 0)
            { yield return CheckSteamSdk(); yield break; }
            if (!host && !client) yield break;
            if (active) { Destroy(gameObject); yield break; }
            active = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            DontDestroyOnLoad(gameObject);
            var session = PrototypeSession.Instance;
            yield return null;
            if (online) yield return EnsureSteamSignIn();
            else yield return EnsureGuestSignIn();
            var captureDirectory = Argument(args, "-td-captures");
            if (!string.IsNullOrEmpty(captureDirectory))
            {
                yield return new WaitForSecondsRealtime(1);
                Capture(System.IO.Path.Combine(captureDirectory, host ? "menu-host.png" : "menu-client.png"));
                yield return new WaitForSecondsRealtime(1);
            }
            string codeFile = Argument(args, "-td-code-file");
            if (online) yield return ConnectThroughOnlineUi(host, host ? "warrior" : "wizard", codeFile, string.Empty, captureDirectory);
            else session.Connect(host, host ? "warrior" : "wizard", "127.0.0.1", 7779);
            double deadline = Time.realtimeSinceStartupAsDouble + 45;
            if (reject)
            {
                while (Time.realtimeSinceStartupAsDouble < deadline && session.Connecting) yield return null;
                bool passed = session.Status.Contains("full") && session.LocalHero == null;
                Debug.Log(passed ? "TD_REJECTION_PASS" : "TD_REJECTION_FAIL " + session.Status);
                Application.Quit(passed ? 0 : 1);
                yield break;
            }
            while ((session.LocalHero == null || session.LocalHero.Definition == null) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            var hero = session.LocalHero;
            if (hero == null) { Fail("No local player."); yield break; }
            if (online && host)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                Click("COPY ROOM CODE");
                if (GUIUtility.systemCopyBuffer != session.JoinCode) { Fail("Copy room code did not copy the current code."); yield break; }
                if (!string.IsNullOrEmpty(captureDirectory)) Capture(System.IO.Path.Combine(captureDirectory, "online-lobby-host.png"));
            }
            hero.SetReadyRpc(true);
            while (!hero.Running.Value && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!hero.Running.Value) { Fail("Both players did not ready up."); yield break; }
            var start = hero.transform.position;
            hero.SmokeInput = Vector2.right;
            yield return new WaitForSecondsRealtime(1.3f);
            hero.SmokeInput = Vector2.zero;
            hero.RequestSlot(1);
            yield return new WaitForSecondsRealtime(2);
            if (session.Heroes.Count != 2 || Vector3.Distance(start, hero.transform.position) < 2 || hero.SelectedSlot.Value != 1)
            { Fail("Movement, roster, or hotbar did not synchronize."); yield break; }
            foreach (var player in session.Heroes)
                if (player.Definition == null || player.SelectedSlot.Value != 1)
                { Fail("Remote class or slot did not synchronize."); yield break; }
            var view = Camera.main;
            Vector3 centered = view.WorldToViewportPoint(hero.transform.position + Vector3.up);
            if (Mathf.Abs(centered.x - 0.5f) > 0.01f || Mathf.Abs(centered.y - 0.5f) > 0.01f)
            { Fail("Camera focus left the hero."); yield break; }
            if (!string.IsNullOrEmpty(captureDirectory))
                Capture(System.IO.Path.Combine(captureDirectory, host ? "arena-host.png" : "arena-client.png"));
            Debug.Log($"TD_SMOKE_PASS role={(host ? "host" : "client")} class={hero.Definition.Id} side={hero.Side.Value} players=2 slot={hero.SelectedSlot.Value} position={hero.transform.position}");
            hero.SmokeInput = null;
            if (Array.IndexOf(args, "-td-combat") >= 0) yield return CheckCombat(hero, captureDirectory, host);
            yield return CheckControls(hero, captureDirectory, host);
            if (Array.IndexOf(args, "-td-economy") >= 0) yield return CheckEconomy(hero, captureDirectory, host);
            if (Array.IndexOf(args, "-td-combat") >= 0) yield return CheckCombatLife(hero, captureDirectory, host);
            if (Array.IndexOf(args, "-td-combat") >= 0) yield return CheckCombatResult(hero, captureDirectory, host);
            // Give both peers time to finish local input checks before resetting shared state.
            yield return new WaitForSecondsRealtime(3);
            if (host) session.ResetLobby();
            deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (hero.Running.Value && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (hero.Running.Value || hero.Ready.Value || hero.SelectedSlot.Value != 0 || hero.Round.Value != 0 ||
                hero.Incoming.Count != 0 || hero.Energy.Count != 0 || hero.Economy.Value.XP != 0 || hero.Economy.Value.Gold != 0 ||
                hero.Plots.Count != 0 || hero.Nodes.Count != 0 || hero.Economy.Value.Stone != 0 || hero.Economy.Value.HeroLevel != 0)
            { Fail("Reset did not clear match state."); yield break; }
            yield return new WaitForSecondsRealtime(0.3f);
            if (Vector3.Distance(hero.transform.position, session.Catalog.TestMap.Lanes[hero.Side.Value].HeroSpawn) > 0.5f)
            { Fail("Reset did not restore the spawn."); yield break; }
            hero.SetReadyRpc(true);
            while (!hero.Running.Value && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!hero.Running.Value) { Fail("Reset match could not start again."); yield break; }
            Debug.Log("TD_RESET_PASS");
            if (!host) yield return new WaitForSecondsRealtime(1);
            if (host)
            {
                deadline = Time.realtimeSinceStartupAsDouble + 15;
                while ((session.Heroes.Count != 1 || hero.Running.Value) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                if (session.Heroes.Count != 1 || hero.Running.Value) { Fail("Disconnect did not restore the lobby."); yield break; }
                Debug.Log("TD_DISCONNECT_PASS");
            }
            string oldCode = session.JoinCode;
            session.Leave();
            deadline = Time.realtimeSinceStartupAsDouble + 15;
            while ((SceneManager.GetActiveScene().name != "MainMenu" || session.Manager.ShutdownInProgress) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            yield return new WaitForSecondsRealtime(host ? 1 : 3);
            if (SceneManager.GetActiveScene().name != "MainMenu" || session.Heroes.Count != 0)
            { Fail("Leave did not clean up the arena."); yield break; }
            // Swap the classes to catch stale loadout state after leaving and starting a new session.
            if (online) yield return ConnectThroughOnlineUi(host, host ? "wizard" : "warrior", codeFile, oldCode, captureDirectory);
            else session.Connect(host, host ? "wizard" : "warrior", "127.0.0.1", 7779);
            deadline = Time.realtimeSinceStartupAsDouble + 30;
            while ((session.LocalHero == null || session.LocalHero.Definition == null) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            hero = session.LocalHero;
            if (hero == null) { Fail("Rejoin did not spawn a new hero."); yield break; }
            hero.SetReadyRpc(true);
            while (!hero.Running.Value && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!hero.Running.Value || session.Heroes.Count != 2 || hero.Definition.Id != (host ? "wizard" : "warrior") || hero.SelectedSlot.Value != 0)
            { Fail("Rejoin kept stale roster, class, or inventory state."); yield break; }
            Debug.Log("TD_REJOIN_PASS");
            yield return new WaitForSecondsRealtime(host ? 2 : 1);
            Application.Quit(0);
        }

        private static IEnumerator CheckCombat(NetworkHero hero, string captures, bool host)
        {
            var session = PrototypeSession.Instance;
            hero.RequestSlot(0);
            yield return new WaitForSecondsRealtime(0.3f);
            hero.RequestSend();
            yield return new WaitForSecondsRealtime(1.7f);
            hero.RequestSend();
            yield return new WaitForSecondsRealtime(0.5f);
            if (hero.Economy.Value.XP != 10 || hero.Incoming.Count != 2 || hero.Round.Value == 0 || hero.MatchPlayerId.Value.IsEmpty)
            { Fail("Combat sends/XP/identity did not replicate."); yield break; }
            if (!host)
                foreach (var other in session.Heroes) if (other != hero && (other.Energy.Count != 0 || other.Economy.Value.XP != 0))
                { Fail("Opponent private balances were replicated."); yield break; }
            hero.RequestAttack(Vector2.up);
            yield return new WaitForSecondsRealtime(0.25f);
            if (hero.Energy.Count != 1 || hero.Energy[0].Current >= hero.Energy[0].Capacity)
            { Fail("Weapon energy cost was not acknowledged."); yield break; }
            var ui = UnityEngine.Object.FindFirstObjectByType<TowerDefense.UI.PrototypeView>();
            ui.OpenPanel("Troops");
            yield return new WaitForSecondsRealtime(0.25f);
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, host ? "combat-host.png" : "combat-client.png"));
            ui.ClosePanel();
            Debug.Log("TD_COMBAT_PASS role=" + (host ? "host" : "client") + " sends=2 xp=10 energy=spent private=owner-only");
        }

        private static IEnumerator ConnectThroughOnlineUi(bool host, string classId, string codeFile, string oldCode, string captures)
        {
            var session = PrototypeSession.Instance;
            Click("PLAY");
            yield return null;
            Click("SELECT", session.Catalog.FindClass(classId).DisplayName);
            yield return null;
            string code = string.Empty;
            if (!host)
            {
                double deadline = Time.realtimeSinceStartupAsDouble + 60;
                while (Time.realtimeSinceStartupAsDouble < deadline)
                {
                    if (System.IO.File.Exists(codeFile)) code = System.IO.File.ReadAllText(codeFile).Trim();
                    if (!string.IsNullOrEmpty(code) && code != oldCode) break;
                    yield return null;
                }
                if (string.IsNullOrEmpty(code) || code == oldCode) { Fail("Host did not publish a fresh Steam room code."); yield break; }
                UnityEngine.Object.FindFirstObjectByType<InputField>().text = code.ToLowerInvariant();
            }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, $"online-connect-{(host ? "host" : "client")}-{classId}.png"));
            Click(host ? "HOST STEAM" : "JOIN STEAM");
            double connectionDeadline = Time.realtimeSinceStartupAsDouble + 60;
            while ((session.Connecting || (host && string.IsNullOrEmpty(session.JoinCode))) &&
                   Time.realtimeSinceStartupAsDouble < connectionDeadline)
            {
                if (!session.Connecting && !session.Manager.IsListening) break;
                yield return null;
            }
            if (host && !string.IsNullOrEmpty(session.JoinCode))
            {
                // Only the shareable code is written; allocation credentials/tokens are never logged.
                System.IO.File.WriteAllText(codeFile, session.JoinCode);
                Debug.Log("TD_STEAM_HOST_READY");
            }
            else if (host || session.LocalHero == null)
                Debug.Log("TD_STEAM_CONNECT_RESULT " + session.Status);
        }

        private static IEnumerator CheckSteamSdk()
        {
            // One developer account and Valve's example app, never a two-player/production claim.
            var service = PrototypeSession.Instance.SteamService;
            var first = service.HostAsync();
            double deadline = Time.realtimeSinceStartupAsDouble + 45;
            while (!first.IsCompleted && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!first.IsCompletedSuccessfully) { service.Leave(); Fail("Steam private SDK check failed (startup/lobby)."); yield break; }
            string firstCode = first.Result.JoinCode;
            bool member = service.IsMember(first.Result.HostSteamId);
            service.Leave();
            var second = service.HostAsync();
            deadline = Time.realtimeSinceStartupAsDouble + 45;
            while (!second.IsCompleted && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            bool passed = second.IsCompletedSuccessfully && service.InitializedAppId == 480 && member && second.Result.JoinCode != firstCode;
            service.Leave();
            Debug.Log(passed ? "TD_STEAM_PRIVATE_PASS SDK-init guest-free-Steam-identity private-lobby leave fresh-rehost app=480 developer-only" : "TD_STEAM_PRIVATE_FAIL");
            Application.Quit(passed ? 0 : 1);
        }

        private static IEnumerator CheckSteamConfiguration(string[] args)
        {
            // The unset preview must show a recoverable error, never fall back to the sample app.
            var settings = Resources.Load<SteamSettings>("SteamSettings");
            if (settings == null || settings.AppId != 0) { Fail("Configuration diagnostic requires the unset preview App ID."); yield break; }
            var session = PrototypeSession.Instance;
            yield return new WaitForSecondsRealtime(0.4f);
            string captures = Argument(args, "-td-captures");
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "startup-guest-choice.png"));
            bool passed = session.SignIn.Flow.State == StartupSignInState.GuestChoice &&
                session.SignIn.Flow.Status.Contains("not configured") && session.CanConnect &&
                !session.Manager.IsListening && session.SteamService.InitializedAppId == 0 &&
                SceneManager.GetActiveScene().name == "MainMenu";
            Click("RETRY STEAM");
            yield return new WaitForSecondsRealtime(0.4f);
            passed &= session.SignIn.Flow.State == StartupSignInState.GuestChoice && session.SteamService.InitializedAppId == 0;
            yield return EnsureGuestSignIn();
            passed &= session.SignIn.Ready && session.SignIn.Account.Provider == "guest" &&
                session.OnlineProvider is EosGuestProvider && session.SteamService.InitializedAppId == 0;
            var profile = session.SignIn.Account;
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "guest-main-menu.png"));
            Click("PLAY"); yield return null;
            Click("SELECT", "Warrior"); yield return null;
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                if (button.name == "HOST GUEST" || button.name == "JOIN GUEST") passed &= button.interactable == session.GuestService.Configured;
            passed &= session.SignIn.Account.Equals(profile) && !session.LocalAccount.IsValid;
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "steam-setup-required.png"));
            Debug.Log(passed ? "TD_STEAM_CONFIG_PASS app=0 no-Steam-native-init startup-retry guest-consent stable-local-profile EOS-provider" : "TD_STEAM_CONFIG_FAIL");
            Application.Quit(passed ? 0 : 1);
        }

        private static IEnumerator CheckSteamPlayableMode(string[] args)
        {
            var session = PrototypeSession.Instance;
            string captures = Argument(args, "-td-captures");
            yield return EnsureSteamSignIn();
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "automatic-steam-main-menu.png"));
            Click("PLAY"); yield return null;
            Click("SELECT", "Warrior"); yield return null;
            if (!session.SteamService.PrivatePlaytest) { Fail("Private mode menu did not activate."); yield break; }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "private-steam-connect.png"));
            Click("HOST STEAM");
            double deadline = Time.realtimeSinceStartupAsDouble + 60;
            while ((session.Connecting || session.LocalHero == null) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!session.Manager.IsHost || session.LocalHero == null || session.SteamService.InitializedAppId != 480 ||
                session.LocalAccount.Provider != "steam-test" || !SteamLobbyCode.TryNormalize(session.JoinCode, out _))
            { session.Leave(); Fail("Private Steam playable host failed: " + session.Status); yield break; }
            string oldCode = session.JoinCode;
            var account = session.LocalAccount;
            yield return new WaitForSecondsRealtime(0.3f);
            Click("COPY ROOM CODE");
            if (GUIUtility.systemCopyBuffer != oldCode) { Fail("Private room code did not copy."); yield break; }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, "private-steam-lobby.png"));
            session.Leave();
            while (!session.CanConnect && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!session.CanConnect || session.LocalAccount.IsValid) { Fail("Private leave did not clean up."); yield break; }
            yield return null;
            Click("PLAY"); yield return null;
            Click("SELECT", "Wizard"); yield return null;
            Click("HOST STEAM");
            deadline = Time.realtimeSinceStartupAsDouble + 60;
            while ((session.Connecting || session.LocalHero == null || session.LocalHero.Definition == null) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            bool passed = session.Manager.IsHost && session.LocalHero != null && session.LocalHero.Definition != null &&
                session.LocalHero.Definition.Id == "wizard" && !string.IsNullOrEmpty(session.JoinCode) && session.JoinCode != oldCode && session.LocalAccount.Equals(account);
            session.Leave();
            while (!session.CanConnect && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            passed &= session.CanConnect && !session.LocalAccount.IsValid && session.SignIn.Ready && session.SignIn.Account.Equals(account);
            Debug.Log(passed ? "TD_STEAM_PLAYTEST_PASS automatic-startup-sign-in menu SDK lobby NGO-host P2P-listen code-copy leave fresh-rehost retained-profile no-second-peer" : "TD_STEAM_PLAYTEST_FAIL");
            Application.Quit(passed ? 0 : 1);
        }

        private static IEnumerator EnsureSteamSignIn()
        {
            var session = PrototypeSession.Instance;
            yield return new WaitForSecondsRealtime(0.4f);
            if (!session.SignIn.Ready)
            {
                Click("ENABLE PRIVATE STEAM TEST (480)");
                yield return new WaitForSecondsRealtime(0.4f);
            }
            if (!session.SignIn.Ready || session.SignIn.Account.Provider != "steam-test" && session.SignIn.Account.Provider != "steam")
                Fail("Startup did not automatically sign in through Steam.");
        }

        private static IEnumerator EnsureGuestSignIn()
        {
            var session = PrototypeSession.Instance;
            yield return new WaitForSecondsRealtime(0.4f);
            if (!session.SignIn.Ready)
            {
                Click("PLAY AS GUEST");
                yield return new WaitForSecondsRealtime(0.2f);
            }
            if (!session.SignIn.Ready) Fail("Startup guest choice did not unlock the main menu.");
        }

        private static void Click(string name, string parent = null)
        {
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (button.name != name || (parent != null && button.transform.parent.name != parent)) continue;
                button.onClick.Invoke();
                return;
            }
            Fail("Missing UI button: " + name);
        }

        private static IEnumerator CheckControls(NetworkHero hero, string captures, bool host)
        {
            hero.RequestSlot(1);
            yield return new WaitForSecondsRealtime(0.3f);
            // Feed virtual devices through the real named actions rather than calling movement/camera methods.
            var keyboard = InputSystem.AddDevice<Keyboard>("Smoke keyboard");
            var mouse = InputSystem.AddDevice<Mouse>("Smoke mouse");
            var controls = PrototypeSession.Instance.Controls;
            var view = Camera.main;
            var orbit = view.GetComponent<HeroOrbitCamera>();
            Vector2 pointer = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer });
            yield return new WaitForSecondsRealtime(0.3f);
            Vector3 stationary = hero.transform.position;
            float yaw = orbit.Yaw;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, delta = new Vector2(host ? 240 : -150, 20) }.WithButton(MouseButton.Right));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer });
            yield return new WaitForSecondsRealtime(0.3f);
            Vector3 centered = view.WorldToViewportPoint(hero.transform.position + Vector3.up);
            if (Mathf.Abs(orbit.Yaw - yaw) < 10 || Vector3.Distance(stationary, hero.transform.position) > 0.2f ||
                Mathf.Abs(centered.x - 0.5f) > 0.01f || Mathf.Abs(centered.y - 0.5f) > 0.01f)
            { Fail("Right-drag orbit moved the hero or lost camera focus."); yield break; }
            Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            yield return new WaitForSecondsRealtime(0.6f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(0.4f);
            Vector3 displacement = hero.transform.position - stationary; displacement.y = 0;
            if (displacement.magnitude < 1 || Vector3.Dot(displacement.normalized, forward) < 0.9f)
            { Fail("WASD did not follow the local camera."); yield break; }
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, scroll = new Vector2(0, -120) });
            yield return new WaitForSecondsRealtime(0.3f);
            if (hero.SelectedSlot.Value != 2) { Fail("Mouse wheel did not select the next hotbar slot."); yield break; }
            foreach (var key in new[] { Key.B, Key.U, Key.T, Key.I, Key.Escape })
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                yield return new WaitForSecondsRealtime(0.15f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return new WaitForSecondsRealtime(0.2f);
                if (!controls.BlockGameplay) { Fail("Panel shortcut did not block gameplay: " + key); yield break; }
                stationary = hero.transform.position; yaw = orbit.Yaw;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, delta = new Vector2(200, 0), scroll = new Vector2(0, -120) }.WithButton(MouseButton.Right));
                yield return new WaitForSecondsRealtime(0.3f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer });
                yield return new WaitForSecondsRealtime(0.2f);
                if (Vector3.Distance(stationary, hero.transform.position) > 0.2f || orbit.Yaw != yaw || hero.SelectedSlot.Value != 2)
                { Fail("A menu leaked movement, orbit, or hotbar input."); yield break; }
                if (key == Key.I && !string.IsNullOrEmpty(captures))
                    Capture(System.IO.Path.Combine(captures, host ? "equipment-host.png" : "equipment-client.png"));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                yield return new WaitForSecondsRealtime(0.15f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return new WaitForSecondsRealtime(0.2f);
                if (controls.BlockGameplay) { Fail("Panel shortcut did not close the panel: " + key); yield break; }
            }
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            Debug.Log("TD_CONTROLS_PASS orbit camera-relative-WASD wheel panels input-blocking");
        }

        private static IEnumerator CheckEconomy(NetworkHero hero, string captures, bool host)
        {
            var session = PrototypeSession.Instance; var rules = session.Catalog.EconomyRules;
            // Explicit authority-only diagnostic teleport. Normal clients cannot choose their position.
            if (host) foreach (var player in session.Heroes)
            {
                int node = Array.FindIndex(session.Catalog.TestMap.NodeSides, s => s == player.Side.Value);
                player.SmokeTeleport(session.Catalog.TestMap.ResourceNodes[node] + Vector3.forward * 2);
            }
            int ownNode = Array.FindIndex(session.Catalog.TestMap.NodeSides, s => s == hero.Side.Value);
            double deadline = Time.realtimeSinceStartupAsDouble + 5;
            while (Vector3.Distance(hero.transform.position, session.Catalog.TestMap.ResourceNodes[ownNode]) > 3 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (hero.Plots.Count != 2 || hero.Nodes.Count != 2 || hero.Economy.Value.Stone != 0 ||
                Vector3.Distance(hero.transform.position, session.Catalog.TestMap.ResourceNodes[ownNode]) > 3)
            { Fail("Economy sites/fixture did not synchronize."); yield break; }
            if (!host) foreach (var other in session.Heroes)
                if (other != hero && (other.Nodes.Count != 0 || other.Economy.Value.Gold != 0 || other.Economy.Value.Stone != 0))
                { Fail("Opponent reserves/materials leaked."); yield break; }
            hero.RequestSlot(1); yield return new WaitForSecondsRealtime(0.3f);
            var keyboard = InputSystem.AddDevice<Keyboard>("Economy smoke keyboard");
            for (int i = 0; i < 4; i++)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); yield return new WaitForSecondsRealtime(0.1f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSecondsRealtime(rules.HarvestSeconds + 0.4f);
                if (hero.Economy.Value.Stone != (i + 1) * rules.HarvestYield) { Fail("Timed mining/input/material replication failed."); yield break; }
            }
            InputSystem.RemoveDevice(keyboard);
            var ui = UnityEngine.Object.FindFirstObjectByType<TowerDefense.UI.PrototypeView>(); ui.OpenPanel("Shop");
            yield return new WaitForSecondsRealtime(0.2f); Click("BUY LAND"); yield return new WaitForSecondsRealtime(0.3f);
            Click("BUILD TOWER"); yield return new WaitForSecondsRealtime(0.3f);
            if (!hero.Plots[0].Owned || hero.Plots[0].TowerIndex != 0 || hero.Economy.Value.Stone != 0)
            { Fail("Shop purchase/construction did not synchronize."); yield break; }
            // Duplicate occupied-slot construction must not consume/refund anything.
            hero.RequestEconomy(EconomyAction.BuildTower, hero.Plots[0].Id);
            yield return new WaitForSecondsRealtime(0.3f);
            if (hero.Economy.Value.Stone != 0) { Fail("Rejected build changed materials."); yield break; }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, host ? "economy-shop-host.png" : "economy-shop-client.png"));
            ui.ClosePanel(); hero.RequestSend(); yield return new WaitForSecondsRealtime(7.5f);
            if (hero.Plots[0].AttackAt < 0) { Fail("Constructed tower never fired."); yield break; }
            if (host) foreach (var player in session.Heroes)
                player.SmokeTeleport(session.Catalog.TestMap.Plots[player.Plots[0].Id] + Vector3.forward * 4);
            yield return new WaitForSecondsRealtime(0.5f);
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, host ? "economy-world-host.png" : "economy-world-client.png"));
            ui.OpenPanel("Shop"); yield return new WaitForSecondsRealtime(0.2f);
            Click("SELL TOWER"); yield return new WaitForSecondsRealtime(0.3f);
            Click("SELL LAND"); yield return new WaitForSecondsRealtime(0.3f);
            Click("GROW HERO (MAX HP)"); yield return new WaitForSecondsRealtime(0.3f);
            if (hero.Plots[0].Owned || hero.Plots[0].TowerIndex != -1 || hero.Economy.Value.Stone != rules.TowerRefundStone ||
                hero.Economy.Value.HeroLevel != 1 || hero.MaximumHealth.Value != hero.Definition.MaxHealth + rules.HeroHealthBonus)
            { Fail("Sales/refunds/hero growth did not synchronize."); yield break; }
            ui.ClosePanel();
            Debug.Log("TD_ECONOMY_PASS timed-input harvest private-reserves buy build fire reject sell grow development-teleport-fixture");
        }

        private static IEnumerator CheckCombatLife(NetworkHero hero, string captures, bool host)
        {
            var session = PrototypeSession.Instance;
            hero.RequestSlot(0); hero.RequestSend();
            yield return new WaitForSecondsRealtime(1);
            if (host)
            {
                // Explicit development-only fixture: lower authority health to make the real troop hit kill promptly.
                // This is not a production RPC or a claim that final combat balance has been tested.
                foreach (var player in session.Heroes)
                {
                    CombatTroop latest = null;
                    foreach (var troop in session.Combat.Troops)
                        if (troop.Lane == player.Side.Value && (latest == null || troop.Id > latest.Id)) latest = troop;
                    if (latest == null) { Fail("No incoming troop for ghost fixture."); yield break; }
                    session.Combat.Players[player.Side.Value].Health = 1;
                    player.SmokeTeleport(session.Combat.Position(latest));
                }
            }
            double deadline = Time.realtimeSinceStartupAsDouble + 8;
            while (hero.Life.Value != HeroLife.Ghost && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (hero.Life.Value != HeroLife.Ghost || hero.GrantsVision) { Fail("Troop death/ghost eligibility did not synchronize."); yield break; }
            int xp = hero.Economy.Value.XP; float energy = hero.Energy[0].Current;
            if (hero.Economy.Value.HeroLevel == 1)
            {
                int stone = hero.Economy.Value.Stone;
                hero.RequestEconomy(EconomyAction.Harvest, hero.Nodes[0].Id);
                hero.RequestEconomy(EconomyAction.BuyPlot, hero.Plots[0].Id);
                yield return new WaitForSecondsRealtime(0.3f);
                if (hero.Economy.Value.Stone != stone || hero.Economy.Value.HarvestEnds != 0 || !hero.Plots[0].Owned)
                { Fail("Ghost mining rejection/management failed."); yield break; }
                hero.RequestEconomy(EconomyAction.SellPlot, hero.Plots[0].Id);
                yield return new WaitForSecondsRealtime(0.3f);
                if (hero.Plots[0].Owned) { Fail("Ghost sale failed."); yield break; }
                Debug.Log("TD_GHOST_ECONOMY_PASS no-harvest buy sell");
            }
            hero.RequestAttack(Vector2.up);
            var start = hero.transform.position; hero.SmokeInput = Vector2.up;
            yield return new WaitForSecondsRealtime(0.5f); hero.SmokeInput = Vector2.zero;
            if (hero.Energy[0].Current != energy || Vector3.Distance(start, hero.transform.position) < 0.5f)
            { Fail("Ghost attack rejection or controllable movement failed."); yield break; }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, host ? "ghost-host.png" : "ghost-client.png"));
            deadline = Time.realtimeSinceStartupAsDouble + 10;
            while (hero.Life.Value == HeroLife.Ghost && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            hero.SmokeInput = null;
            yield return new WaitForSecondsRealtime(0.25f);
            if (hero.Life.Value != HeroLife.Alive || hero.Health.Value != hero.MaximumHealth.Value || hero.Economy.Value.XP != xp ||
                Vector3.Distance(hero.transform.position, session.Catalog.TestMap.Lanes[hero.Side.Value].HeroSpawn) > 0.5f)
            { Fail("Home respawn did not preserve economy/restore health."); yield break; }
            Debug.Log("TD_GHOST_PASS death movement attack-rejection no-vision timed-home-respawn economy-preserved development-fixture");
        }

        private static IEnumerator CheckCombatResult(NetworkHero hero, string captures, bool host)
        {
            yield return new WaitForSecondsRealtime(1);
            var session = PrototypeSession.Instance;
            if (host)
            {
                // Development fixture advances one authoritative troop near a weakened castle.
                var match = session.Combat;
                var sender = match.Players[0];
                if (!match.TrySend(sender.Id, match.Generation, sender.LastSequence + 1, out _))
                { Fail("Result fixture could not send."); yield break; }
                var troop = match.Troops[match.Troops.Count - 1];
                troop.Health = 1000;
                troop.SpawnAt = match.Time - Vector3.Distance(match.Map.Lanes[1].Entry, match.Map.Lanes[1].Castle) / match.Rules.TroopSpeed + 0.1;
                match.Castles[1] = 1;
            }
            double deadline = Time.realtimeSinceStartupAsDouble + 10;
            while (hero.Phase.Value != CombatPhase.Finished && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (hero.Phase.Value != CombatPhase.Finished || hero.Winner.Value != 0)
            { Fail("Castle victory/result did not synchronize."); yield break; }
            yield return new WaitForSecondsRealtime(0.3f);
            int xp = hero.Economy.Value.XP; hero.RequestSend();
            yield return new WaitForSecondsRealtime(0.3f);
            if (hero.Economy.Value.XP != xp) { Fail("Post-result send changed XP."); yield break; }
            if (!string.IsNullOrEmpty(captures)) Capture(System.IO.Path.Combine(captures, host ? "results-host.png" : "results-client.png"));
            Debug.Log("TD_RESULT_PASS castle winner post-result-rejection development-fixture");
        }

        private static string Argument(string[] args, string key)
        {
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : string.Empty;
        }

        internal static void Capture(string path)
        {
            // Hidden test windows can skip presenting frames. Render the live scene/UI explicitly for QA.
            var camera = Camera.main;
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var changed = new System.Collections.Generic.List<Canvas>();
            var texture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            Texture2D pixels = null;
            try
            {
                foreach (var canvas in canvases)
                {
                    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                    changed.Add(canvas);
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1;
                }
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = texture });
                RenderTexture.active = texture;
                pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                pixels.Apply();
                System.IO.File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                foreach (var canvas in changed)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.worldCamera = null;
                }
                Canvas.ForceUpdateCanvases();
                if (pixels != null) UnityEngine.Object.Destroy(pixels);
                RenderTexture.ReleaseTemporary(texture);
            }
        }

        private static void Fail(string reason) { Debug.LogError("TD_SMOKE_FAIL " + reason); Application.Quit(1); }
#endif
    }
}
