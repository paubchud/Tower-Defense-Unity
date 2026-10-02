using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Networking
{
    public sealed class SteamP2PTransport : NetworkTransport
    {
        private sealed class Peer
        {
            public ulong Id;
            public HSteamNetConnection Handle;
            public uint NextSequence, LastSequence;
            public bool HasSequence;
        }

        public ulong HostSteamId;
        public Func<ulong, bool> IsRoomMember;
        private readonly Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        private readonly Queue<SteamNetConnectionStatusChangedCallback_t> changes = new Queue<SteamNetConnectionStatusChangedCallback_t>();
        private readonly IntPtr[] messages = new IntPtr[1];
        private Callback<SteamNetConnectionStatusChangedCallback_t> callback;
        private HSteamListenSocket listenSocket;
        private bool server;
        public override ulong ServerClientId => 0;

        public override void Initialize(NetworkManager networkManager = null) { }

        private void Subscribe()
        {
            changes.Clear();
            callback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(change => changes.Enqueue(change));
            SteamNetworkingUtils.InitRelayNetworkAccess();
        }

        public override bool StartServer()
        {
            Subscribe();
            server = true;
            listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, Array.Empty<SteamNetworkingConfigValue_t>());
            if (listenSocket.m_HSteamListenSocket != 0) return true;
            Shutdown();
            return false;
        }

        public override bool StartClient()
        {
            if (HostSteamId == 0) return false;
            Subscribe();
            server = false;
            var identity = new SteamNetworkingIdentity();
            identity.SetSteamID(new CSteamID(HostSteamId));
            var handle = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, Array.Empty<SteamNetworkingConfigValue_t>());
            if (handle.m_HSteamNetConnection == 0) { Shutdown(); return false; }
            peers[0] = new Peer { Id = 0, Handle = handle };
            return true;
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = 0; payload = default; receiveTime = Time.realtimeSinceStartup;
            while (changes.Count > 0)
            {
                var change = changes.Dequeue();
                ulong remote = change.m_info.m_identityRemote.GetSteamID64();
                ulong id = server ? remote : 0;
                if (server && change.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
                {
                    if (!change.m_info.m_hListenSocket.Equals(listenSocket)) continue;
                    if (peers.Count >= 1 || peers.ContainsKey(id) || IsRoomMember == null || !IsRoomMember(remote))
                    {
                        SteamNetworkingSockets.CloseConnection(change.m_hConn, 0, "Room full or not a member", false);
                        continue;
                    }
                    if (SteamNetworkingSockets.AcceptConnection(change.m_hConn) == EResult.k_EResultOK)
                        peers[id] = new Peer { Id = id, Handle = change.m_hConn };
                    else SteamNetworkingSockets.CloseConnection(change.m_hConn, 0, "Could not accept connection", false);
                    continue;
                }
                if (!peers.TryGetValue(id, out var peer) || !peer.Handle.Equals(change.m_hConn)) continue;
                clientId = id;
                if (change.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
                    return NetworkEvent.Connect;
                if (change.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer ||
                    change.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
                {
                    SteamNetworkingSockets.CloseConnection(peer.Handle, 0, "Connection ended", false);
                    peers.Remove(id);
                    return NetworkEvent.Disconnect;
                }
            }
            foreach (var peer in peers.Values)
            {
                if (SteamNetworkingSockets.ReceiveMessagesOnConnection(peer.Handle, messages, 1) <= 0) continue;
                try
                {
                    var message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[0]);
                    if (message.m_cbSize < SteamPacket.HeaderSize || message.m_cbSize > SteamPacket.MaximumPayload + SteamPacket.HeaderSize) continue;
                    var packet = new byte[message.m_cbSize];
                    Marshal.Copy(message.m_pData, packet, 0, packet.Length);
                    if (!SteamPacket.TryDecode(packet, ref peer.HasSequence, ref peer.LastSequence, out payload)) continue;
                    clientId = peer.Id;
                    return NetworkEvent.Data;
                }
                finally { SteamNetworkingMessage_t.Release(messages[0]); }
            }
            return NetworkEvent.Nothing;
        }

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery delivery)
        {
            if (!peers.TryGetValue(clientId, out var peer)) return;
            bool sequenced = delivery == NetworkDelivery.UnreliableSequenced;
            byte[] packet = SteamPacket.Encode(payload, sequenced, sequenced ? peer.NextSequence++ : 0);
            bool reliable = delivery == NetworkDelivery.Reliable || delivery == NetworkDelivery.ReliableSequenced || delivery == NetworkDelivery.ReliableFragmentedSequenced;
            int flags = reliable ? Constants.k_nSteamNetworkingSend_ReliableNoNagle : Constants.k_nSteamNetworkingSend_UnreliableNoNagle;
            var pin = GCHandle.Alloc(packet, GCHandleType.Pinned);
            try
            {
                var result = SteamNetworkingSockets.SendMessageToConnection(peer.Handle, pin.AddrOfPinnedObject(), (uint)packet.Length, flags, out _);
                if (result != EResult.k_EResultOK) Debug.LogWarning("TD_STEAM_SEND_FAILED result=" + result);
            }
            finally { pin.Free(); }
        }

        public override ulong GetCurrentRtt(ulong clientId) => 0; // No measured ping HUD yet; zero is not a latency guarantee.

        public override void DisconnectLocalClient() => ClosePeer(0);
        public override void DisconnectRemoteClient(ulong clientId) => ClosePeer(clientId);
        private void ClosePeer(ulong id)
        {
            if (!peers.TryGetValue(id, out var peer)) return;
            SteamNetworkingSockets.CloseConnection(peer.Handle, 0, "Disconnected", false);
            peers.Remove(id);
        }

        public override void Shutdown()
        {
            foreach (var peer in peers.Values) SteamNetworkingSockets.CloseConnection(peer.Handle, 0, "Session closed", false);
            peers.Clear();
            if (listenSocket.m_HSteamListenSocket != 0) SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = default;
            callback?.Dispose(); callback = null;
            changes.Clear();
            server = false;
        }
    }
}
