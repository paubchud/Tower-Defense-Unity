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
using Unity.Services.Core;
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
        public IRelayConnector Relay { get; set; }
        public NetworkHero LocalHero => Heroes.Find(hero => hero != null && hero.IsOwner);
        private UnityTransport transport;
        private readonly Dictionary<ulong, string> approved = new Dictionary<ulong, string>();
        private readonly Dictionary<ulong, int> sides = new Dictionary<ulong, int>();
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
            Relay = new RelayConnector();
            transport = gameObject.AddComponent<UnityTransport>();
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
            approved.Clear(); sides.Clear(); Heroes.Clear();
            Controls.BlockGameplay = false;
            Manager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes("td-smoke-1|" + classId);
            return true;
        }

        private bool IsCurrent(int attempt) => this != null && !stopping && attempt == connectionAttempt;

        private IEnumerator ConnectRoutine(bool host, string address, ushort port, int attempt)
        {
            Status = host ? "Opening your arena..." : "Connecting to the host...";
            transport.SetConnectionData(address, port, "0.0.0.0");
            yield return SceneManager.LoadSceneAsync("TestArena");
            if (IsCurrent(attempt)) StartNetwork(host, attempt);
        }

        public async Task ConnectOnlineAsync(bool host, string classId, string code = "")
        {
            if (!CanConnect) return;
            if (!host && !RelayJoinCode.TryNormalize(code, out code))
            { Status = "Enter the 6-12 character room code from your host."; return; }
            if (!BeginConnection(classId, true)) return;
            int attempt = connectionAttempt;
            var cancellation = new CancellationTokenSource();
            preparationCancellation = cancellation;
            Status = host ? "Creating your online room..." : "Looking up the online room...";
            try
            {
                Task<RelayConnection> request = host ? Relay.HostAsync() : Relay.JoinAsync(code);
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
                transport.SetRelayServerData(connection.ServerData);
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
                ServiceErrorCode = error is RequestFailedException failure ? failure.ErrorCode : (int?)null;
                Status = RelayConnector.DescribeFailure(error);
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

        private static async Task ObserveAbandonedRequest(Task<RelayConnection> request)
        {
            try { await request; } catch { /* Canceled attempts cannot update UI or start networking. */ }
        }

        private bool StartNetwork(bool host, int attempt)
        {
            bool started = host ? Manager.StartHost() : Manager.StartClient();
            if (!started)
            {
                Connecting = false;
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
            JoinCode = string.Empty;
            Status = "The host did not respond. Return to the menu and check the room code or address.";
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = request.Payload == null || request.Payload.Length > 128
                ? string.Empty : Encoding.UTF8.GetString(request.Payload);
            string classId = payload.StartsWith("td-smoke-1|") ? payload.Substring(11) : string.Empty;
            response.Approved = Catalog.FindClass(classId) != null && (approved.ContainsKey(request.ClientNetworkId) || approved.Count < 2);
            response.Pending = false;
            response.CreatePlayerObject = response.Approved;
            if (!response.Approved)
            {
                response.Reason = approved.Count >= 2 ? "This 1v1 arena is full." : "Class or game version does not match the host.";
                return;
            }
            int side = sides.TryGetValue(request.ClientNetworkId, out int existing) ? existing : (sides.ContainsValue(0) ? 1 : 0);
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
                sides.Remove(id);
                foreach (var hero in Heroes)
                    if (hero != null && hero.IsSpawned && hero.OwnerClientId != id) hero.ResetForLobby();
                Status = "Opponent disconnected. Waiting for a replacement.";
            }
            else
            {
                bool wasJoining = Connecting;
                Connecting = false;
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
            if (!Manager.IsServer || Heroes.Count != 2) return;
            foreach (var hero in Heroes)
                if (!hero.IsSpawned || !hero.Ready.Value) return;
            foreach (var hero in Heroes) hero.Running.Value = true;
            Status = "Arena open. Explore your side and the opponent's side.";
            Debug.Log("TD_MATCH_STARTED players=2");
        }

        public void ResetLobby()
        {
            if (!Manager.IsServer) return;
            foreach (var hero in Heroes) if (hero != null && hero.IsSpawned) hero.ResetForLobby();
            Status = "Arena reset. Both players can ready up again.";
        }

        public void Leave()
        {
            if (!stopping) StartCoroutine(LeaveRoutine());
        }

        private IEnumerator LeaveRoutine()
        {
            stopping = true;
            connectionAttempt++;
            preparationCancellation?.Cancel();
            Connecting = false;
            JoinCode = string.Empty;
            Online = false;
            Manager.Shutdown();
            while (Manager.ShutdownInProgress) yield return null;
            approved.Clear(); sides.Clear(); Heroes.Clear();
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
            (Relay as IDisposable)?.Dispose();
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
