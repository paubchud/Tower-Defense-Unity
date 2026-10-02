using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace TowerDefense.Networking
{
    public readonly struct SteamRoom
    {
        public readonly string JoinCode;
        public readonly ulong HostSteamId;
        public SteamRoom(string code, ulong host) { JoinCode = code; HostSteamId = host; }
    }

    public interface ISteamRooms
    {
        Task<SteamRoom> HostAsync();
        Task<SteamRoom> JoinAsync(string code);
        void Leave();
        bool IsMember(ulong steamId);
    }

    public sealed class SteamOnlineException : Exception
    {
        public SteamOnlineException(string message) : base(message) { }
    }

    public static class SteamLobbyCode
    {
        public static bool TryNormalize(string input, out string code)
        {
            code = string.Empty;
            string candidate = input?.Trim();
            if (string.IsNullOrEmpty(candidate) || candidate.Length > 20) return false;
            foreach (char value in candidate) if (value < '0' || value > '9') return false;
            if (!ulong.TryParse(candidate, out ulong id) || !new CSteamID(id).IsLobby()) return false;
            code = id.ToString();
            return true;
        }
    }

    public sealed class SteamLobbyService : MonoBehaviour, ISteamRooms
    {
        public string PendingInvite { get; private set; } = string.Empty;
        public event Action HostLost;
        private const string GameId = "tower-defense-pvp";
        private readonly List<IDisposable> calls = new List<IDisposable>();
        private Callback<GameLobbyJoinRequested_t> inviteCallback;
        private CSteamID lobby;
        private CSteamID originalOwner;
        private int generation;
        private bool initialized;
        private float nextOwnerCheck;
        public uint InitializedAppId { get; private set; }

        private void Awake()
        {
            var args = Environment.GetCommandLineArgs();
            int launchInvite = Array.IndexOf(args, "+connect_lobby");
            if (launchInvite >= 0 && launchInvite + 1 < args.Length && SteamLobbyCode.TryNormalize(args[launchInvite + 1], out var code)) PendingInvite = code;
            var settings = Resources.Load<SteamSettings>("SteamSettings");
            if (!Application.isEditor && !Application.isBatchMode && settings != null && settings.AppId != 0)
            {
                try { InitializeSteam(); }
                catch (Exception error) { Debug.LogWarning("TD_STEAM_STARTUP_FAILED type=" + error.GetType().Name); }
            }
        }

        private void InitializeSteam()
        {
            if (initialized) return;
            var settings = Resources.Load<SteamSettings>("SteamSettings");
            uint appId = settings != null ? settings.AppId : 0;
            bool privateTest = (Application.isEditor || Debug.isDebugBuild) && Array.IndexOf(Environment.GetCommandLineArgs(), "-td-steam-private-test") >= 0;
            if (privateTest) appId = 480; // Valve's example identity is never selected for normal play or a release.
            if (appId == 0) throw new SteamOnlineException("Steam is not configured yet. Set this game's App ID in the SteamSettings asset. LAN testing still works.");
            if (appId == 480 && !privateTest) throw new SteamOnlineException("App ID 480 is reserved for explicit private SDK tests. Configure your game's own Steamworks App ID.");
            Environment.SetEnvironmentVariable("SteamAppId", appId.ToString(), EnvironmentVariableTarget.Process);
            if (!SteamAPI.Init()) throw new SteamOnlineException("Could not initialize Steam. Open Steam, sign in, and check access to the game's App ID.");
            initialized = true;
            InitializedAppId = SteamUtils.GetAppID().m_AppId;
            if (InitializedAppId != appId || !SteamUser.BLoggedOn())
            {
                SteamAPI.Shutdown(); initialized = false;
                throw new SteamOnlineException("Steam must be signed in using the configured game App ID.");
            }
            inviteCallback = Callback<GameLobbyJoinRequested_t>.Create(invite => PendingInvite = invite.m_steamIDLobby.m_SteamID.ToString());
        }

        private void Update()
        {
            if (!initialized) return;
            SteamAPI.RunCallbacks();
            if (lobby.m_SteamID == 0 || Time.unscaledTime < nextOwnerCheck) return;
            nextOwnerCheck = Time.unscaledTime + 0.5f;
            if (SteamMatchmaking.GetLobbyOwner(lobby) != originalOwner)
            {
                Leave();
                HostLost?.Invoke(); // Never silently turn the remaining client into a new authority.
            }
        }

        private Task<T> AwaitCall<T>(SteamAPICall_t handle) where T : struct
        {
            if (handle.m_SteamAPICall == 0) throw new SteamOnlineException("Steam could not begin that room request. Try again.");
            var completion = new TaskCompletionSource<T>();
            CallResult<T> result = null;
            result = CallResult<T>.Create((data, failed) =>
            {
                calls.Remove(result); result.Dispose();
                if (failed) completion.TrySetException(new SteamOnlineException("Steam's room service did not respond. Check your connection and try again."));
                else completion.TrySetResult(data);
            });
            calls.Add(result); result.Set(handle);
            return completion.Task;
        }

        public async Task<SteamRoom> HostAsync()
        {
            InitializeSteam();
            CheckPendingRequests();
            int request = ++generation;
            var created = await AwaitCall<LobbyCreated_t>(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2));
            if (created.m_eResult != EResult.k_EResultOK) throw new SteamOnlineException("Steam could not create the room. Check your connection and retry.");
            var room = new CSteamID(created.m_ulSteamIDLobby);
            if (request != generation) { SteamMatchmaking.LeaveLobby(room); throw new OperationCanceledException(); }
            if (!SteamMatchmaking.SetLobbyData(room, "td-game", GameId) || !SteamMatchmaking.SetLobbyData(room, "td-version", Application.version))
            { SteamMatchmaking.LeaveLobby(room); throw new SteamOnlineException("Steam could not initialize the room. Try hosting again."); }
            lobby = room;
            originalOwner = SteamUser.GetSteamID();
            PendingInvite = string.Empty;
            return new SteamRoom(room.m_SteamID.ToString(), originalOwner.m_SteamID);
        }

        public async Task<SteamRoom> JoinAsync(string code)
        {
            InitializeSteam();
            if (!SteamLobbyCode.TryNormalize(code, out code)) throw new SteamOnlineException("Enter the numeric Steam room code from your friend.");
            CheckPendingRequests();
            int request = ++generation;
            var entered = await AwaitCall<LobbyEnter_t>(SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(code))));
            if (entered.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                throw new SteamOnlineException(entered.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseFull
                    ? "This 1v1 arena is full." : "Could not join that Steam room. Check the code, Steam friendship/invite, and host connection.");
            var room = new CSteamID(entered.m_ulSteamIDLobby);
            if (request != generation) { SteamMatchmaking.LeaveLobby(room); throw new OperationCanceledException(); }
            SteamMatchmaking.RequestLobbyData(room);
            for (int retry = 0; retry < 25 && (SteamMatchmaking.GetLobbyData(room, "td-game").Length == 0 || SteamMatchmaking.GetLobbyData(room, "td-version").Length == 0); retry++)
                await Task.Delay(80);
            if (request != generation) { SteamMatchmaking.LeaveLobby(room); throw new OperationCanceledException(); }
            if (SteamMatchmaking.GetLobbyData(room, "td-game") != GameId || SteamMatchmaking.GetLobbyData(room, "td-version") != Application.version)
            { SteamMatchmaking.LeaveLobby(room); throw new SteamOnlineException("This room belongs to a different game or update. Both players must use the same build."); }
            var owner = SteamMatchmaking.GetLobbyOwner(room);
            if (owner == SteamUser.GetSteamID())
            { SteamMatchmaking.LeaveLobby(room); throw new SteamOnlineException("Join from a different Steam account/device. One account cannot be both players."); }
            lobby = room; originalOwner = owner; PendingInvite = string.Empty;
            return new SteamRoom(room.m_SteamID.ToString(), owner.m_SteamID);
        }

        private void CheckPendingRequests()
        {
            // Keep late callbacks for cleanup, but rapid cancel/retry cannot grow them without limit.
            if (calls.Count >= 4) throw new SteamOnlineException("Steam is still finishing previous room requests. Wait a moment before retrying.");
        }

        public bool IsMember(ulong steamId)
        {
            if (!initialized || lobby.m_SteamID == 0) return false;
            int count = SteamMatchmaking.GetNumLobbyMembers(lobby);
            for (int i = 0; i < count; i++) if (SteamMatchmaking.GetLobbyMemberByIndex(lobby, i).m_SteamID == steamId) return true;
            return false;
        }

        public bool ShowInviteOverlay()
        {
            if (!initialized || lobby.m_SteamID == 0 || !SteamUtils.IsOverlayEnabled()) return false;
            SteamFriends.ActivateGameOverlayInviteDialog(lobby);
            return true;
        }

        public void Leave()
        {
            generation++;
            if (initialized && lobby.m_SteamID != 0) SteamMatchmaking.LeaveLobby(lobby);
            lobby = default; originalOwner = default;
        }

        public static string DescribeFailure(Exception error) => error is SteamOnlineException known ? known.Message
            : error is TimeoutException ? "Steam connection timed out. Check Steam and your internet connection, then retry."
            : "Could not connect through Steam. Check Steam, the game's App ID, and the room code or invite.";

        private void OnDestroy()
        {
            Leave();
            GetComponent<SteamP2PTransport>()?.Shutdown();
            foreach (var call in calls) call.Dispose();
            calls.Clear(); inviteCallback?.Dispose();
            if (initialized) SteamAPI.Shutdown();
            initialized = false;
        }
    }
}
