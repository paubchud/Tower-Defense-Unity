using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Input;
using TowerDefense.Presentation;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Networking
{
    [DefaultExecutionOrder(-1000)]
    public sealed class PrototypeSession : MonoBehaviour
    {
        public static PrototypeSession Instance { get; private set; }
        public ContentCatalog Catalog;
        public GameObject HeroPrefab;
        public PrototypeInput Controls { get; private set; }
        public NetworkManager Manager { get; private set; }
        public readonly List<NetworkHero> Heroes = new List<NetworkHero>(2);
        public string Status { get; private set; } = "Choose a class to begin.";
        public bool Connecting { get; private set; }
        public bool Online { get; private set; }
        public string JoinCode { get; private set; } = string.Empty;
        public int? ServiceErrorCode { get; private set; }
        public bool CanConnect => !Connecting && !stopping && Manager != null && !Manager.IsListening && !Manager.ShutdownInProgress;
        public IOnlineProvider OnlineProvider { get; private set; }
        public AccountIdentity LocalAccount { get; private set; }
        public SteamLobbyService SteamService { get; private set; }
        public PlayerSignInService SignIn { get; private set; }
        public EosGuestService GuestService { get; private set; }
        public MatchFormat Format => MatchFormat.Duel; // Eight-player content/map/authority gates must pass before enabling FourTeamDuos.
        public NetworkHero LocalHero => Heroes.Find(hero => hero != null && hero.IsOwner);
        public CombatMatch Combat { get; private set; }
        private uint roundGeneration;
        private double lastCombatTick, nextCombatReplication;
        private UnityTransport transport;
        private SteamP2PTransport steamTransport;
        private EosP2PTransport guestTransport;
        private readonly Dictionary<ulong, string> approved = new Dictionary<ulong, string>();
        private readonly Dictionary<ulong, int> sides = new Dictionary<ulong, int>();
        private readonly TeamSeatAllocator teamSeats = new TeamSeatAllocator(MatchFormat.Duel);
        private bool stopping;
        private int connectionAttempt;
        private CancellationTokenSource preparationCancellation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            Controls = gameObject.AddComponent<PrototypeInput>();
            SteamService = gameObject.AddComponent<SteamLobbyService>();
            GuestService = gameObject.AddComponent<EosGuestService>();
            guestTransport = gameObject.AddComponent<EosP2PTransport>();
            transport = gameObject.AddComponent<UnityTransport>();
            steamTransport = gameObject.AddComponent<SteamP2PTransport>();
            steamTransport.IsRoomMember = SteamService.IsMember;
            OnlineProvider = new SteamOnlineProvider(SteamService, steamTransport);
            OnlineProvider.HostLost += OnlineHostLost;
            Manager = gameObject.AddComponent<NetworkManager>();
            Manager.NetworkConfig = new NetworkConfig
            {
                PlayerPrefab = HeroPrefab, NetworkTransport = transport,
                ConnectionApproval = true, EnableSceneManagement = false, TickRate = 30,
                ClientConnectionBufferTimeout = 10
            };
            Manager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = HeroPrefab });
            Manager.ConnectionApprovalCallback = Approve;
            Manager.OnClientConnectedCallback += Connected;
            Manager.OnClientDisconnectCallback += Disconnected;
            SignIn = gameObject.AddComponent<PlayerSignInService>();
            SignIn.Initialize(SteamService);
            if (Application.isEditor || Debug.isDebugBuild) gameObject.AddComponent<TowerDefense.Diagnostics.EosSmokeTest>();
        }

        public bool ContinueAsGuest()
        {
            if (!CanConnect || !SignIn.ContinueAsGuest()) return false;
            SetOnlineProvider(new EosGuestProvider(GuestService, guestTransport));
            var args = Environment.GetCommandLineArgs();
            bool localDiagnostic = (Application.isEditor || Debug.isDebugBuild) &&
                (Array.IndexOf(args, "-td-steam-config-test") >= 0 || Array.IndexOf(args, "-td-smoke-host") >= 0 || Array.IndexOf(args, "-td-smoke-client") >= 0);
            if (GuestService.Configured && !localDiagnostic) _ = WarmGuestLogin();
            return true;
        }

        private async Task WarmGuestLogin() { try { await GuestService.SignInAsync(); } catch { /* Guest UI shows a sanitized retryable status; LAN still works. */ } }

        public void SetOnlineProvider(IOnlineProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (!CanConnect) throw new InvalidOperationException("Leave the current connection before changing provider.");
            OnlineProvider.HostLost -= OnlineHostLost;
            OnlineProvider = provider;
            OnlineProvider.HostLost += OnlineHostLost;
            LocalAccount = default;
        }

        public bool EnablePrivateSteamPlaytest()
        {
            if (!CanConnect || !(OnlineProvider is SteamOnlineProvider)) return false;
            try { SteamService.EnablePrivatePlaytest(); Status = "Private Steam test enabled (Spacewar App ID 480). Both players must enable this mode; use matching builds and separate Steam accounts."; return true; }
            catch (SteamOnlineException error) { Status = error.Message; return false; }
        }

        public void Connect(bool host, string classId, string address, ushort port)
        {
            if (!CanConnect) return;
            string target = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim();
            if (port == 0 || !IPAddress.TryParse(target, out var parsed) || parsed.AddressFamily != AddressFamily.InterNetwork)
            { Status = "Enter a valid IPv4 address and a port between 1 and 65535."; return; }
            if (!BeginConnection(classId, false)) return;
            StartCoroutine(ConnectRoutine(host, target, port, connectionAttempt));
        }

        private bool BeginConnection(string classId, bool online)
        {
            if (!CanConnect) return false;
            if (Catalog.FindClass(classId) == null) { Status = "Unknown class."; return false; }
            connectionAttempt++;
            Connecting = true;
            Online = online;
            JoinCode = string.Empty;
            ServiceErrorCode = null;
            LocalAccount = default;
            approved.Clear(); sides.Clear(); teamSeats.Clear(); Heroes.Clear();
            Combat = null;
            Controls.BlockGameplay = false;
            Manager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes("td-game-" + Application.version + "|" + classId);
            return true;
        }

        private bool IsCurrent(int attempt) => this != null && !stopping && attempt == connectionAttempt;

        private IEnumerator ConnectRoutine(bool host, string address, ushort port, int attempt)
        {
            Status = host ? "Opening your arena..." : "Connecting to the host...";
            Manager.NetworkConfig.NetworkTransport = transport;
            transport.SetConnectionData(address, port, "0.0.0.0");
            yield return SceneManager.LoadSceneAsync("TestArena");
            if (IsCurrent(attempt)) StartNetwork(host, attempt);
        }

        public async Task ConnectOnlineAsync(bool host, string classId, string code = "")
        {
            if (!CanConnect) return;
            if (!host && !OnlineProvider.TryNormalizeCode(code, out code))
            { Status = "Enter a valid " + OnlineProvider.DisplayName + " room code from your host."; return; }
            if (!BeginConnection(classId, true)) return;
            int attempt = connectionAttempt;
            var cancellation = new CancellationTokenSource();
            var provider = OnlineProvider;
            preparationCancellation = cancellation;
            Status = host ? "Creating your online room..." : "Looking up the online room...";
            try
            {
                Task<OnlineConnection> request = provider.PrepareAsync(host, code);
                Task timeout = Task.Delay(TimeSpan.FromSeconds(45), cancellation.Token);
                if (await Task.WhenAny(request, timeout) != request)
                {
                    // The service SDK cannot cancel an in-flight request. Observe its result but never apply it.
                    _ = ObserveAbandonedRequest(request);
                    cancellation.Token.ThrowIfCancellationRequested();
                    throw new TimeoutException();
                }
                var connection = await request;
                if (!IsCurrent(attempt)) return;
                if (connection.Transport == null || !connection.Account.IsValid) throw new InvalidOperationException("Provider returned an incomplete connection.");
                Manager.NetworkConfig.NetworkTransport = connection.Transport;
                LocalAccount = connection.Account;
                Status = "Opening the online arena...";
                var loading = SceneManager.LoadSceneAsync("TestArena");
                while (!loading.isDone) await Task.Yield();
                if (!IsCurrent(attempt)) return;
                if (StartNetwork(host, attempt)) JoinCode = connection.JoinCode;
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                if (!IsCurrent(attempt)) return;
                Connecting = false;
                JoinCode = string.Empty;
                provider.Leave();
                LocalAccount = default;
                ServiceErrorCode = null;
                Status = provider.DescribeFailure(error);
                // Service messages can include response bodies: only log their type/code, never credentials.
                Debug.LogWarning($"TD_ONLINE_CONNECT_FAILED type={error.GetType().Name} code={ServiceErrorCode}");
                if (Manager.IsListening) Manager.Shutdown();
            }
            finally
            {
                if (ReferenceEquals(preparationCancellation, cancellation)) preparationCancellation = null;
                cancellation.Cancel();
                cancellation.Dispose();
            }
        }

        private static async Task ObserveAbandonedRequest(Task<OnlineConnection> request)
        {
            try { await request; } catch { /* Canceled attempts cannot update UI or start networking. */ }
        }

        private bool StartNetwork(bool host, int attempt)
        {
            bool started = host ? Manager.StartHost() : Manager.StartClient();
            if (!started)
            {
                Connecting = false;
                if (Online) OnlineProvider.Leave();
                LocalAccount = default;
                Status = "Could not start the connection. Return to the menu and try again.";
            }
            else if (host)
            {
                Connecting = false;
                Status = "Waiting for a second player. Both players must ready up.";
            }
            else
            {
                Status = "Joining the arena...";
                StartCoroutine(ConnectionDeadline(attempt));
            }
            return started;
        }

        private IEnumerator ConnectionDeadline(int attempt)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (IsCurrent(attempt) && Connecting && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (!IsCurrent(attempt) || !Connecting) yield break;
            Connecting = false;
            Manager.Shutdown();
            if (Online) OnlineProvider.Leave();
            LocalAccount = default;
            JoinCode = string.Empty;
            Status = "The host did not respond. Return to the menu and check the room code or address.";
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = request.Payload == null || request.Payload.Length > 128
                ? string.Empty : Encoding.UTF8.GetString(request.Payload);
            string prefix = "td-game-" + Application.version + "|";
            string classId = payload.StartsWith(prefix, StringComparison.Ordinal) ? payload.Substring(prefix.Length) : string.Empty;
            response.Approved = Catalog.FindClass(classId) != null && (approved.ContainsKey(request.ClientNetworkId) || approved.Count < Format.TotalPlayers);
            response.Pending = false;
            response.CreatePlayerObject = response.Approved;
            if (!response.Approved)
            {
                response.Reason = approved.Count >= Format.TotalPlayers ? "This 1v1 arena is full." : "Class or game version does not match the host.";
                return;
            }
            if (!teamSeats.TryGet(request.ClientNetworkId, out var seat))
            {
                if (!teamSeats.TryAssignParty(new[] { request.ClientNetworkId }))
                { response.Approved = response.CreatePlayerObject = false; response.Reason = "No team slot is available."; return; }
                teamSeats.TryGet(request.ClientNetworkId, out seat);
            }
            int side = seat.Team;
            approved[request.ClientNetworkId] = classId;
            sides[request.ClientNetworkId] = side;
            response.Position = Catalog.TestMap.Lanes[side].HeroSpawn;
            response.Rotation = Quaternion.identity;
        }

        private void Connected(ulong id)
        {
            if (id == Manager.LocalClientId) Connecting = false;
            if (Manager.IsServer && approved.TryGetValue(id, out var classId))
            {
                var hero = Manager.ConnectedClients[id].PlayerObject.GetComponent<NetworkHero>();
                hero.Initialize(classId, sides[id]);
            }
            Status = "Connected. Ready up when both players have joined.";
        }

        private void Disconnected(ulong id)
        {
            if (stopping) return;
            if (Manager.IsServer)
            {
                // Rejected/unapproved connections must not reset the two accepted players' match.
                if (!approved.Remove(id)) return;
                Combat = null;
                sides.Remove(id);
                teamSeats.Remove(id);
                foreach (var hero in Heroes)
                    if (hero != null && hero.IsSpawned && hero.OwnerClientId != id) hero.ResetForLobby();
                Status = "Opponent disconnected. Waiting for a replacement.";
            }
            else
            {
                bool wasJoining = Connecting;
                Connecting = false;
                if (Online) OnlineProvider.Leave();
                LocalAccount = default;
                JoinCode = string.Empty;
                string reason = Manager.DisconnectReason;
                if (string.IsNullOrEmpty(reason) || reason.StartsWith("[Disconnect Event]"))
                    Status = wasJoining ? "Could not join. The room may be full or the host disconnected. Return to the menu and try a fresh code."
                        : "Connection ended. Return to the menu to reconnect.";
                else Status = reason; // Explicit game approval messages, such as the two-player limit.
            }
        }

        public void TryBeginMatch()
        {
            if (!Manager.IsServer || Heroes.Count != Format.TotalPlayers) return;
            if (Combat != null) return;
            foreach (var hero in Heroes)
                if (!hero.IsSpawned || !hero.Ready.Value) return;
            var classes = new HeroClassDefinition[2];
            foreach (var hero in Heroes) classes[hero.Side.Value] = hero.Definition;
            try { Combat = new CombatMatch(Catalog.CombatRules, Catalog.TestMap, classes, ++roundGeneration, Manager.ServerTime.Time); }
            catch (ArgumentException error) { Status = "Combat content is invalid: " + error.Message; Debug.LogError(Status); return; }
            lastCombatTick = Manager.ServerTime.Time; nextCombatReplication = 0;
            foreach (var hero in Heroes) hero.SyncCombat(Combat);
            foreach (var hero in Heroes) hero.Running.Value = true;
            Status = "Match started. Send troops with T, upgrade with U, and hold left mouse to attack.";
            Debug.Log("TD_MATCH_STARTED players=2");
        }

        private void FixedUpdate()
        {
            if (Combat == null || !Manager.IsServer || !Manager.IsListening) return;
            double now = Manager.ServerTime.Time;
            double remaining = Math.Min(0.25, Math.Max(0, now - lastCombatTick));
            lastCombatTick = now;
            foreach (var hero in Heroes) if (hero != null && hero.IsSpawned) Combat.UpdateHero(hero.Side.Value, hero.transform.position, hero.SelectedSlot.Value);
            // Bound each simulation step; a stall does not create a single huge physics/combat step.
            while (remaining > 0.000001)
            { float step = (float)Math.Min(remaining, 0.05); Combat.Step(step); remaining -= step; }
            // Death/respawn must affect movement immediately, not at the slower list/economy cadence.
            foreach (var hero in Heroes) if (hero != null && hero.IsSpawned) hero.SyncLife(Combat);
            if (now < nextCombatReplication) return;
            nextCombatReplication = now + 0.1; // Changes only; lane position is evaluated locally from spawn time.
            foreach (var hero in Heroes) if (hero != null && hero.IsSpawned) hero.SyncCombat(Combat);
            if (Combat.Phase == CombatPhase.Finished)
                Status = Combat.Winner < 0 ? "Draw: both castles fell." : "Side " + (Combat.Winner + 1) + " wins! Host can reset for a rematch.";
        }

        public void ResetLobby()
        {
            if (!Manager.IsServer) return;
            Combat = null;
            foreach (var hero in Heroes) if (hero != null && hero.IsSpawned) hero.ResetForLobby();
            Status = "Arena reset. Both players can ready up again.";
        }

        public void Leave()
        {
            if (!stopping) StartCoroutine(LeaveRoutine());
        }

        private void OnlineHostLost()
        {
            if (!Online || stopping) return;
            Connecting = false;
            JoinCode = string.Empty;
            LocalAccount = default;
            Manager.Shutdown();
            Status = "Host left the room. Return to the menu to reconnect.";
        }

        public void InviteFriend()
        {
            if (OnlineProvider.ShowInviteOverlay()) return;
            Status = "The invitation overlay is unavailable here. Share the room code with your friend instead.";
        }

        private IEnumerator LeaveRoutine()
        {
            stopping = true;
            Combat = null;
            connectionAttempt++;
            preparationCancellation?.Cancel();
            OnlineProvider.Leave();
            LocalAccount = default;
            Connecting = false;
            JoinCode = string.Empty;
            Online = false;
            Manager.Shutdown();
            while (Manager.ShutdownInProgress) yield return null;
            approved.Clear(); sides.Clear(); teamSeats.Clear(); Heroes.Clear();
            Controls.BlockGameplay = false;
            yield return SceneManager.LoadSceneAsync("MainMenu");
            Status = "Choose a class to begin.";
            stopping = false;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            connectionAttempt++;
            preparationCancellation?.Cancel();
            OnlineProvider?.Leave();
            if (OnlineProvider != null) OnlineProvider.HostLost -= OnlineHostLost;
            if (Manager != null)
            {
                Manager.OnClientConnectedCallback -= Connected;
                Manager.OnClientDisconnectCallback -= Disconnected;
            }
            WorldGeometry.ClearMaterials();
            Instance = null;
        }
    }
}
