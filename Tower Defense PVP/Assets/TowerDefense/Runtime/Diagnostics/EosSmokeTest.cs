using System;
using System.Collections;
using System.Threading.Tasks;
using Epic.OnlineServices;
using TowerDefense.Networking;
using UnityEngine;

namespace TowerDefense.Diagnostics
{
    public sealed class EosSmokeTest : MonoBehaviour
    {
        private IEnumerator Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!(Application.isEditor || Debug.isDebugBuild) || Array.IndexOf(Environment.GetCommandLineArgs(), "-td-eos-check") < 0) yield break;
            var session = PrototypeSession.Instance;
            yield return new WaitForSecondsRealtime(0.7f);
            if (session.SignIn.Ready || !session.ContinueAsGuest()) { Fail("Expected an explicit guest startup with App ID unset."); yield break; }
            var profile = session.SignIn.Account;
            var arguments = Environment.GetCommandLineArgs();
            int captureIndex = Array.IndexOf(arguments, "-td-eos-captures");
            string captures = captureIndex >= 0 && captureIndex + 1 < arguments.Length ? arguments[captureIndex + 1] : null;
            Task signIn = session.GuestService.SignInAsync();
            yield return WaitFor(signIn, 55);
            if (!signIn.IsCompleted || signIn.IsFaulted || !session.GuestService.Authenticated)
            { Fail(session.GuestService.Status); yield break; }
            if (session.GuestService.OnlineAccount.Provider != "eos-guest" || profile.Provider != "guest" || session.SteamService.InitializedAppId != 0)
            { Fail("Guest authentication depended on Steam or confused profile/PUID namespaces."); yield break; }
            if (captures != null)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                SmokeTestDriver.Capture(System.IO.Path.Combine(captures, "guest-eos-main-menu.png"));
                yield return new WaitForSecondsRealtime(0.5f);
            }
            Task canceled = session.GuestService.PrepareRoomAsync(true, "");
            session.GuestService.Leave(); // Cancel before the native create callback, not after successful hosting.
            yield return WaitFor(canceled, 30);
            Task<Result> canceledCleanup = session.GuestService.LastCleanup;
            yield return WaitFor(canceledCleanup, 30);
            if (!canceled.IsCanceled || !string.IsNullOrEmpty(session.GuestService.RoomCode)
                || !canceledCleanup.IsCompleted || canceledCleanup.Result != Result.Success)
            { Fail("Canceled guest create was applied or failed late-room cleanup."); yield break; }
            Task host = session.ConnectOnlineAsync(true, "warrior");
            yield return WaitFor(host, 55);
            if (!session.Manager.IsHost || session.LocalHero == null || !EosRoomCode.TryNormalize(session.JoinCode, out string first))
            { Fail(session.Status); yield break; }
            if (captures != null)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                SmokeTestDriver.Capture(System.IO.Path.Combine(captures, "guest-eos-room.png"));
                yield return new WaitForSecondsRealtime(0.5f);
            }
            session.ResetLobby();
            session.Leave();
            double deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (!session.CanConnect && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Task<Result> cleanup = session.GuestService.LastCleanup;
            yield return WaitFor(cleanup, 30);
            if (!session.CanConnect || !cleanup.IsCompleted || cleanup.Result != Result.Success || !session.SignIn.Account.Equals(profile))
            { Fail("Guest leave, lobby destruction or profile retention failed."); yield break; }
            Task rehost = session.ConnectOnlineAsync(true, "wizard");
            yield return WaitFor(rehost, 55);
            if (!session.Manager.IsHost || !EosRoomCode.TryNormalize(session.JoinCode, out var fresh) || fresh == first)
            { Fail("Guest fresh rehost failed: " + session.Status); yield break; }
            session.Leave();
            yield return new WaitForSecondsRealtime(0.5f);
            cleanup = session.GuestService.LastCleanup;
            yield return WaitFor(cleanup, 30);
            if (!cleanup.IsCompleted || cleanup.Result != Result.Success) { Fail("Final guest room cleanup failed."); yield break; }
            Debug.Log("TD_EOS_SMOKE_PASS DeviceID Connect lobby NGO-host relay-config cancel-late-cleanup leave fresh-rehost namespaces no-Steam no-second-peer");
            Application.Quit(0);
#else
            yield break;
#endif
        }
        private static IEnumerator WaitFor(Task task, double seconds)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + seconds;
            while (!task.IsCompleted && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (task.IsFaulted) _ = task.Exception; // Observe errors; never print response/credential bodies.
        }
        private static void Fail(string safeMessage) { Debug.LogError("TD_EOS_SMOKE_FAIL " + safeMessage); Application.Quit(1); }
    }
}
