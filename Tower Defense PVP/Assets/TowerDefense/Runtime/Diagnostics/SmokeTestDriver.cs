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
            yield return CheckControls(hero, captureDirectory, host);
            // Give both peers time to finish local input checks before resetting shared state.
            yield return new WaitForSecondsRealtime(3);
            if (host) session.ResetLobby();
            deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (hero.Running.Value && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (hero.Running.Value || hero.Ready.Value || hero.SelectedSlot.Value != 0)
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
