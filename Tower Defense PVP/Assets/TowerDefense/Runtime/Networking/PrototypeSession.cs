using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        public NetworkHero LocalHero => Heroes.Find(hero => hero != null && hero.IsOwner);
        private UnityTransport transport;
        private readonly Dictionary<ulong, string> approved = new Dictionary<ulong, string>();
        private readonly Dictionary<ulong, int> sides = new Dictionary<ulong, int>();
        private bool stopping;

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
            if (Connecting || stopping || Manager.IsListening) return;
            if (Catalog.FindClass(classId) == null) { Status = "Unknown class."; return; }
            string target = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim();
            if (port == 0 || !IPAddress.TryParse(target, out var parsed) || parsed.AddressFamily != AddressFamily.InterNetwork)
            { Status = "Enter a valid IPv4 address and a port between 1 and 65535."; return; }
            StartCoroutine(ConnectRoutine(host, classId, target, port));
        }

        private IEnumerator ConnectRoutine(bool host, string classId, string address, ushort port)
        {
            Connecting = true;
            Status = host ? "Opening your arena..." : "Connecting to the host...";
            approved.Clear(); sides.Clear(); Heroes.Clear();
            Controls.BlockGameplay = false;
            Manager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes("td-smoke-1|" + classId);
            transport.SetConnectionData(string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim(), port, "0.0.0.0");
            yield return SceneManager.LoadSceneAsync("TestArena");
            bool started = host ? Manager.StartHost() : Manager.StartClient();
            if (!started) Status = "Could not start the connection. Check the address and port.";
            else Status = host ? "Waiting for a second player. Both players must ready up." : "Joining the arena...";
            Connecting = false;
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
                Status = string.IsNullOrEmpty(Manager.DisconnectReason) ? "Connection ended. Return to the menu to reconnect." : Manager.DisconnectReason;
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
