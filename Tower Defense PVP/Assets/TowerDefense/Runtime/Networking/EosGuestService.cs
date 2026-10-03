using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.P2P;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Networking
{
    public sealed class EosOnlineException : Exception
    {
        public EosOnlineException(string safeMessage) : base(safeMessage) { }
    }

    public static class EosRoomCode
    {
        public static string Create() => "TDG" + Guid.NewGuid().ToString("N").ToUpperInvariant();
        public static bool TryNormalize(string input, out string code)
        {
            code = input?.Trim().ToUpperInvariant() ?? string.Empty;
            if (code.Length != 35 || !code.StartsWith("TDG", StringComparison.Ordinal)) { code = string.Empty; return false; }
            for (int i = 3; i < code.Length; i++) if (!Uri.IsHexDigit(code[i])) { code = string.Empty; return false; }
            return true;
        }
        public static string SocketName(string code)
        {
            if (!TryNormalize(code, out code)) throw new ArgumentException("Invalid guest room code.");
            return "TD" + code.Substring(3, 24); // Room-specific socket; stale matches cannot inject packets.
        }
    }

    // No Auth Interface/Epic account UI, Steam, overlay, RTC or paid infrastructure.
    // Local guest GUID and this authenticated ProductUserId are deliberately different identities.
    public sealed class EosGuestService : MonoBehaviour
    {
        public ProductUserId UserId { get; private set; }
        public ProductUserId HostId { get; private set; }
        public PlatformInterface Platform { get; private set; }
        public P2PInterface P2P => Platform?.GetP2PInterface();
        public bool Configured => settings != null && settings.IsConfigured;
        public bool Authenticated => UserId != null && UserId.IsValid();
        public string Status { get; private set; } = EosSettings.MissingMessage;
        public string RoomCode { get; private set; } = string.Empty;
        public AccountIdentity OnlineAccount => Authenticated ? new AccountIdentity("eos-guest", UserId.ToString()) : default;
        public event Action HostLost;
        public Task<Result> LastCleanup { get; private set; } = Task.FromResult(Result.Success);
        private EosSettings settings;
        private bool initialized, destroyed, roomBusy;
        private int roomGeneration, pending;
        private Task authentication;
        private float nextMemberCheck;
        private readonly HashSet<string> members = new HashSet<string>();
        private ulong authExpiration, loginStatusNotification;
#if UNITY_EDITOR
        private EosNativeEditor nativeLibrary;
#endif
        private string Bucket => "td-pvp-guest-" + Application.version + "-" + MatchFormat.Duel.Id;

        private void Awake() { settings = EosSettings.Load(); if (Configured) Status = "Guest EOS login will start after Play as Guest."; }

        public Task SignInAsync()
        {
            if (Authenticated) return Task.CompletedTask;
            if (authentication != null && !authentication.IsCompleted) return authentication;
            authentication = AuthenticateAsync();
            return authentication;
        }

        private void InitializeSdk()
        {
            if (initialized) return;
            if (!Configured) throw new EosOnlineException(EosSettings.MissingMessage);
#if UNITY_EDITOR
            nativeLibrary = new EosNativeEditor();
            Bindings.Hook(nativeLibrary, (library, symbol) => library.Symbol(symbol));
#endif
            var init = new InitializeOptions { ProductName = "Tower Defense PVP", ProductVersion = Application.version };
            Check(PlatformInterface.Initialize(ref init), "initialize SDK");
            initialized = true;
            var options = new Options
            {
                ProductId = settings.productId, SandboxId = settings.sandboxId, DeploymentId = settings.deploymentId,
                ClientCredentials = new ClientCredentials { ClientId = settings.clientId, ClientSecret = settings.clientSecret },
                IsServer = false, Flags = PlatformFlags.DisableOverlay | (Application.isEditor ? PlatformFlags.LoadingInEditor : 0),
                CacheDirectory = Path.Combine(Application.persistentDataPath, "EOSCache"), TickBudgetInMilliseconds = 2,
                TaskNetworkTimeoutSeconds = 20
            };
            Platform = PlatformInterface.Create(ref options);
            if (Platform == null) throw new EosOnlineException("EOS platform creation failed. Check the local game-client configuration.");
            var relay = new SetRelayControlOptions { RelayControl = settings.forceRelay ? RelayControl.ForceRelays : RelayControl.AllowRelays };
            Check(P2P.SetRelayControl(ref relay), "configure relay");
            var queues = new SetPacketQueueSizeOptions { IncomingPacketQueueMaxSizeBytes = 1024 * 1024, OutgoingPacketQueueMaxSizeBytes = 1024 * 1024 };
            Check(P2P.SetPacketQueueSize(ref queues), "bound packet queues");
        }

        private async Task AuthenticateAsync()
        {
            Status = "Signing in as an EOS guest...";
            try
            {
                InitializeSdk();
                var completion = NewRequest<Result>();
                var device = new CreateDeviceIdOptions { DeviceModel = SystemInfo.operatingSystemFamily.ToString() };
                Platform.GetConnectInterface().CreateDeviceId(ref device, null, (ref CreateDeviceIdCallbackInfo data) => Complete(completion, data.ResultCode));
                Result result = await Deadline(completion.Task);
                if (result != Result.Success && result != Result.DuplicateNotAllowed) Check(result, "create device identity");
                await LoginAsync(true);
                if (destroyed) throw new OperationCanceledException();
                var expiration = new AddNotifyAuthExpirationOptions();
                if (authExpiration == 0)
                    authExpiration = Platform.GetConnectInterface().AddNotifyAuthExpiration(ref expiration, null,
                        (ref AuthExpirationCallbackInfo data) => { if (data.LocalUserId == UserId) _ = RefreshAsync(); });
                var status = new AddNotifyLoginStatusChangedOptions();
                if (loginStatusNotification == 0)
                    loginStatusNotification = Platform.GetConnectInterface().AddNotifyLoginStatusChanged(ref status, null,
                        (ref LoginStatusChangedCallbackInfo data) =>
                        {
                            if (data.LocalUserId == UserId && data.CurrentStatus != LoginStatus.LoggedIn)
                                AuthenticationLost("EOS guest authentication ended. Return to the menu and retry.");
                        });
                Status = "Signed in as an EOS guest. Use Host Guest or Join Guest with a matching build.";
                Debug.Log("TD_EOS_AUTH_PASS device-login no-account-ui");
            }
            catch (Exception error) { Status = DescribeFailure(error); throw; }
        }

        private async Task LoginAsync(bool allowCreate)
        {
            var completion = NewRequest<LoginCallbackInfo>();
            var login = new LoginOptions
            {
                Credentials = new Credentials { Type = ExternalCredentialType.DeviceidAccessToken, Token = null },
                UserLoginInfo = new UserLoginInfo { DisplayName = "Guest" }
            };
            Platform.GetConnectInterface().Login(ref login, null, (ref LoginCallbackInfo data) => Complete(completion, data));
            var loggedIn = await Deadline(completion.Task);
            ProductUserId identity = loggedIn.LocalUserId;
            if (loggedIn.ResultCode == Result.InvalidUser && allowCreate && loggedIn.ContinuanceToken != null)
            {
                var created = NewRequest<CreateUserCallbackInfo>();
                var create = new CreateUserOptions { ContinuanceToken = loggedIn.ContinuanceToken };
                Platform.GetConnectInterface().CreateUser(ref create, null, (ref CreateUserCallbackInfo data) => Complete(created, data));
                var user = await Deadline(created.Task);
                Check(user.ResultCode, "create guest user"); identity = user.LocalUserId;
            }
            else Check(loggedIn.ResultCode, "sign in guest");
            if (destroyed || identity == null || !identity.IsValid()) throw new EosOnlineException("EOS returned no valid guest identity.");
            if (UserId != null && UserId != identity) throw new EosOnlineException("Guest online identity changed unexpectedly. Restart before continuing.");
            UserId = identity;
        }

        private async Task RefreshAsync()
        {
            try { await LoginAsync(false); }
            catch { if (!destroyed) AuthenticationLost("EOS guest login could not be refreshed. Leave and retry online play."); }
        }
        private void AuthenticationLost(string message)
        {
            Status = message; Leave(); UserId = null; HostLost?.Invoke();
        }

        private TaskCompletionSource<T> NewRequest<T>()
        {
            if (destroyed) throw new OperationCanceledException();
            if (pending >= 8) throw new EosOnlineException("EOS has too many pending requests. Wait, or restart if the service is unreachable.");
            pending++; return new TaskCompletionSource<T>();
        }
        private void Complete<T>(TaskCompletionSource<T> completion, T value) { pending--; completion.TrySetResult(value); }
        private static async Task<T> Deadline<T>(Task<T> task)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(25))) != task) throw new TimeoutException();
            return await task;
        }
        private static void Check(Result result, string operation)
        {
            if (result != Result.Success) throw new EosOnlineException("EOS could not " + operation + " (" + result + "). Check connection, client credentials and Peer2Peer policy.");
        }

        public async Task<string> PrepareRoomAsync(bool host, string code)
        {
            if (roomBusy || !string.IsNullOrEmpty(RoomCode) || !LastCleanup.IsCompleted)
                throw new EosOnlineException("The previous guest room is still opening/closing. Wait before retrying.");
            roomBusy = true;
            int generation = ++roomGeneration;
            string acquired = null;
            try
            {
                await SignInAsync(); RequireCurrent(generation);
                var request = new MatchmakingRequest(MatchFormat.Duel,
                    new PartySnapshot("solo", OnlineAccount, new[] { OnlineAccount }, MatchFormat.Duel), MatchEntryKind.PrivateRoom, Application.version);
                request.RequirePlayablePrivateRoom(); // Deliberate gate; no accidental eight-player/queue admission.
                if (host)
                {
                    var created = NewRequest<CreateLobbyCallbackInfo>();
                    var create = new CreateLobbyOptions
                    {
                        LocalUserId = UserId, MaxLobbyMembers = (uint)request.Format.TotalPlayers,
                        PermissionLevel = LobbyPermissionLevel.Publicadvertised, BucketId = Bucket,
                        DisableHostMigration = true, AllowInvites = false, PresenceEnabled = false,
                        EnableRTCRoom = false, LobbyId = EosRoomCode.Create()
                    };
                    Platform.GetLobbyInterface().CreateLobby(ref create, null, (ref CreateLobbyCallbackInfo data) =>
                    {
                        if (destroyed || generation != roomGeneration)
                        {
                            if (data.ResultCode == Result.Success) CleanupRoom(data.LobbyId.ToString(), true);
                            pending--; created.TrySetCanceled();
                        }
                        else Complete(created, data);
                    });
                    var result = await Deadline(created.Task); Check(result.ResultCode, "create room");
                    acquired = result.LobbyId.ToString(); RequireCurrent(generation);
                    await SetRoomMetadata(acquired); RequireCurrent(generation);
                }
                else
                {
                    if (!EosRoomCode.TryNormalize(code, out code)) throw new EosOnlineException("Enter the full TDG guest room code from your friend.");
                    var searchOptions = new CreateLobbySearchOptions { MaxResults = 1 };
                    Check(Platform.GetLobbyInterface().CreateLobbySearch(ref searchOptions, out var search), "create room lookup");
                    try
                    {
                        var target = new LobbySearchSetLobbyIdOptions { LobbyId = code };
                        Check(search.SetLobbyId(ref target), "select room");
                        var found = NewRequest<Result>();
                        var find = new LobbySearchFindOptions { LocalUserId = UserId };
                        search.Find(ref find, null, (ref LobbySearchFindCallbackInfo data) => Complete(found, data.ResultCode));
                        Check(await Deadline(found.Task), "find room"); RequireCurrent(generation);
                        var copy = new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = 0 };
                        Check(search.CopySearchResultByIndex(ref copy, out var details), "read room");
                        try
                        {
                            ValidateRoom(details);
                            var entered = NewRequest<JoinLobbyCallbackInfo>();
                            var join = new JoinLobbyOptions { LocalUserId = UserId, LobbyDetailsHandle = details, PresenceEnabled = false };
                            Platform.GetLobbyInterface().JoinLobby(ref join, null, (ref JoinLobbyCallbackInfo data) =>
                            {
                                if (destroyed || generation != roomGeneration)
                                {
                                    if (data.ResultCode == Result.Success) CleanupRoom(data.LobbyId.ToString(), false);
                                    pending--; entered.TrySetCanceled();
                                }
                                else Complete(entered, data);
                            });
                            var result = await Deadline(entered.Task); Check(result.ResultCode, "join room"); acquired = result.LobbyId.ToString();
                        }
                        finally { details.Release(); }
                    }
                    finally { search.Release(); }
                    RequireCurrent(generation);
                }
                RoomCode = acquired;
                RefreshMembers(true);
                Status = "Guest room ready. Share the full TDG code. Both players must use this build.";
                Debug.Log("TD_EOS_ROOM_PASS role=" + (host ? "host" : "client") + " capacity=2");
                return RoomCode;
            }
            catch
            {
                if (acquired != null) CleanupRoom(acquired, host);
                if (generation == roomGeneration) { ++roomGeneration; RoomCode = string.Empty; HostId = null; members.Clear(); }
                throw;
            }
            finally { roomBusy = false; }
        }

        private void RequireCurrent(int generation) { if (destroyed || generation != roomGeneration) throw new OperationCanceledException(); }
        private async Task SetRoomMetadata(string room)
        {
            var update = new UpdateLobbyModificationOptions { LocalUserId = UserId, LobbyId = room };
            Check(Platform.GetLobbyInterface().UpdateLobbyModification(ref update, out var modification), "edit room");
            try
            {
                foreach (var attribute in new[] { new[] { "td-game", "tower-defense-pvp" }, new[] { "td-version", Application.version },
                    new[] { "td-pool", "guest" }, new[] { "td-format", MatchFormat.Duel.Id }, new[] { "td-entry", "private-room" } })
                {
                    var add = new LobbyModificationAddAttributeOptions
                    {
                        Attribute = new AttributeData { Key = attribute[0], Value = attribute[1] }, Visibility = LobbyAttributeVisibility.Public
                    };
                    Check(modification.AddAttribute(ref add), "set room metadata");
                }
                var completion = NewRequest<Result>();
                var publish = new UpdateLobbyOptions { LobbyModificationHandle = modification };
                Platform.GetLobbyInterface().UpdateLobby(ref publish, null, (ref UpdateLobbyCallbackInfo data) => Complete(completion, data.ResultCode));
                Check(await Deadline(completion.Task), "publish room metadata");
            }
            finally { modification.Release(); }
        }

        private ProductUserId ValidateRoom(LobbyDetails details)
        {
            var copy = new LobbyDetailsCopyInfoOptions(); Check(details.CopyInfo(ref copy, out var info), "read room info");
            if (!info.HasValue || info.Value.MaxMembers != MatchFormat.Duel.TotalPlayers || info.Value.BucketId.ToString() != Bucket
                || ReadAttribute(details, "td-game") != "tower-defense-pvp" || ReadAttribute(details, "td-version") != Application.version
                || ReadAttribute(details, "td-pool") != "guest" || ReadAttribute(details, "td-format") != MatchFormat.Duel.Id
                || ReadAttribute(details, "td-entry") != "private-room" || info.Value.AllowHostMigration)
                throw new EosOnlineException("That guest room is not a matching 1v1 build/pool. Ask your friend for a fresh code.");
            return info.Value.LobbyOwnerUserId;
        }
        private static string ReadAttribute(LobbyDetails details, string key)
        {
            var read = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = key };
            return details.CopyAttributeByKey(ref read, out var attribute) == Result.Success && attribute.HasValue
                ? attribute.Value.Data?.Value.AsUtf8?.ToString() : null;
        }

        private void RefreshMembers(bool first = false)
        {
            if (!Authenticated || string.IsNullOrEmpty(RoomCode)) return;
            var copy = new CopyLobbyDetailsHandleOptions { LocalUserId = UserId, LobbyId = RoomCode };
            Check(Platform.GetLobbyInterface().CopyLobbyDetailsHandle(ref copy, out var details), "read joined room");
            try
            {
                var owner = ValidateRoom(details);
                if (HostId != null && HostId != owner) throw new EosOnlineException("The original guest host left. Host migration is not enabled.");
                HostId = owner;
                members.Clear();
                var countOptions = new LobbyDetailsGetMemberCountOptions(); uint count = details.GetMemberCount(ref countOptions);
                for (uint i = 0; i < count; i++)
                {
                    var index = new LobbyDetailsGetMemberByIndexOptions { MemberIndex = i };
                    var member = details.GetMemberByIndex(ref index);
                    if (member != null && member.IsValid()) members.Add(member.ToString());
                }
                if (!members.Contains(UserId.ToString()) || !members.Contains(HostId.ToString()))
                    throw new EosOnlineException("Guest room membership ended. Leave and retry.");
            }
            finally { details.Release(); }
        }
        public bool IsMember(ProductUserId id) => id != null && members.Contains(id.ToString());
        public bool CheckCurrentMembership(ProductUserId id)
        {
            try { RefreshMembers(); return IsMember(id); } catch { return false; }
        }

        public void Leave()
        {
            ++roomGeneration;
            string room = RoomCode; bool host = HostId != null && HostId == UserId;
            RoomCode = string.Empty; HostId = null; members.Clear();
            if (!string.IsNullOrEmpty(room)) CleanupRoom(room, host);
        }
        private void CleanupRoom(string room, bool host)
        {
            if (Platform == null || UserId == null) return;
            var cleanup = new TaskCompletionSource<Result>();
            LastCleanup = cleanup.Task;
            if (host)
            {
                var destroy = new DestroyLobbyOptions { LocalUserId = UserId, LobbyId = room };
                Platform.GetLobbyInterface().DestroyLobby(ref destroy, null, (ref DestroyLobbyCallbackInfo data) =>
                {
                    cleanup.TrySetResult(data.ResultCode);
                    Debug.Log("TD_EOS_ROOM_CLEANUP action=destroy result=" + data.ResultCode);
                });
            }
            else
            {
                var leave = new LeaveLobbyOptions { LocalUserId = UserId, LobbyId = room };
                Platform.GetLobbyInterface().LeaveLobby(ref leave, null, (ref LeaveLobbyCallbackInfo data) =>
                {
                    cleanup.TrySetResult(data.ResultCode);
                    Debug.Log("TD_EOS_ROOM_CLEANUP action=leave result=" + data.ResultCode);
                });
            }
        }

        private void Update()
        {
            Platform?.Tick();
            if (Time.unscaledTime < nextMemberCheck || string.IsNullOrEmpty(RoomCode)) return;
            nextMemberCheck = Time.unscaledTime + 0.5f;
            try { RefreshMembers(); }
            catch (Exception error) { Status = DescribeFailure(error); Leave(); HostLost?.Invoke(); }
        }
        public static string DescribeFailure(Exception error)
        {
            if (error is EosOnlineException safe) return safe.Message;
            if (error is TimeoutException) return "EOS did not respond in time. Check the connection and retry with a fresh room code.";
            if (error is OperationCanceledException) return "Guest connection canceled.";
            return "Guest networking could not start. Check SDK/configuration and retry. No profile data was changed.";
        }
        private void OnDestroy()
        {
            Leave(); destroyed = true;
            if (Platform != null)
            {
                var connect = Platform.GetConnectInterface();
                if (authExpiration != 0) connect.RemoveNotifyAuthExpiration(authExpiration);
                if (loginStatusNotification != 0) connect.RemoveNotifyLoginStatusChanged(loginStatusNotification);
                Platform.Release(); Platform = null;
            }
            if (initialized) { PlatformInterface.Shutdown(); initialized = false; }
#if UNITY_EDITOR
            Bindings.Unhook(); nativeLibrary?.Dispose();
#endif
        }
    }

    public sealed class EosGuestProvider : IOnlineProvider
    {
        private readonly EosGuestService service;
        private readonly EosP2PTransport transport;
        public EosGuestProvider(EosGuestService service, EosP2PTransport transport) { this.service = service; this.transport = transport; }
        public string DisplayName => "Guest";
        public string PendingInvite => string.Empty;
        public bool SupportsFriendInvites => false;
        public event Action HostLost { add => service.HostLost += value; remove => service.HostLost -= value; }
        public bool TryNormalizeCode(string input, out string code) => EosRoomCode.TryNormalize(input, out code);
        public async Task<OnlineConnection> PrepareAsync(bool host, string code)
        {
            string room = await service.PrepareRoomAsync(host, code);
            transport.Configure(service, room);
            return new OnlineConnection(room, transport, service.OnlineAccount);
        }
        public void Leave() { transport.Shutdown(); service.Leave(); }
        public bool ShowInviteOverlay() => false;
        public string DescribeFailure(Exception error) => EosGuestService.DescribeFailure(error);
    }
}
