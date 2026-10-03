using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using TowerDefense.Core;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Networking
{
    public sealed class EosP2PTransport : NetworkTransport
    {
        private sealed class Peer
        {
            public ulong Id;
            public ProductUserId User;
            public bool Connected;
            public uint MessageId;
            public readonly EosPacket.Receiver Receiver = new EosPacket.Receiver();
        }
        private readonly Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        private readonly Queue<KeyValuePair<ulong, NetworkEvent>> events = new Queue<KeyValuePair<ulong, NetworkEvent>>();
        private readonly byte[] incoming = new byte[EosPacket.FrameSize];
        private EosGuestService service;
        private SocketId socket;
        private ulong incomingNotification, establishedNotification, closedNotification;
        private bool running, server;
        private ulong nextPeer = 1;
        public override ulong ServerClientId => 0;

        public void Configure(EosGuestService guest, string room)
        {
            if (running) throw new InvalidOperationException("Stop the transport before changing guest rooms.");
            service = guest; socket = new SocketId { SocketName = EosRoomCode.SocketName(room) };
        }
        public override void Initialize(NetworkManager networkManager = null) { }
        private bool StartTransport(bool asServer)
        {
            if (running || service == null || !service.Authenticated || service.HostId == null || string.IsNullOrEmpty(socket.SocketName)) return false;
            server = asServer; running = true; nextPeer = 1; peers.Clear(); events.Clear();
            var request = new AddNotifyPeerConnectionRequestOptions { LocalUserId = service.UserId, SocketId = socket };
            incomingNotification = service.P2P.AddNotifyPeerConnectionRequest(ref request, null, IncomingConnection);
            var established = new AddNotifyPeerConnectionEstablishedOptions { LocalUserId = service.UserId, SocketId = socket };
            establishedNotification = service.P2P.AddNotifyPeerConnectionEstablished(ref established, null, Established);
            var closed = new AddNotifyPeerConnectionClosedOptions { LocalUserId = service.UserId, SocketId = socket };
            closedNotification = service.P2P.AddNotifyPeerConnectionClosed(ref closed, null, Closed);
            if (incomingNotification == 0 || establishedNotification == 0 || closedNotification == 0) { Shutdown(); return false; }
            return true;
        }
        public override bool StartServer() => StartTransport(true);
        public override bool StartClient()
        {
            if (!StartTransport(false)) return false;
            if (service.HostId == service.UserId) { Shutdown(); return false; } // EOS device identity cannot connect to itself.
            peers[0] = new Peer { Id = 0, User = service.HostId };
            var accept = new AcceptConnectionOptions { LocalUserId = service.UserId, RemoteUserId = service.HostId, SocketId = socket };
            if (service.P2P.AcceptConnection(ref accept) != Result.Success || !SendHello(service.HostId)) { Shutdown(); return false; }
            return true;
        }
        private bool SameSocket(SocketId? other) => other.HasValue && other.Value.SocketName == socket.SocketName;
        private Peer Find(ProductUserId user)
        {
            foreach (var peer in peers.Values) if (peer.User == user) return peer;
            return null;
        }
        private void IncomingConnection(ref OnIncomingConnectionRequestInfo data)
        {
            if (!running || !SameSocket(data.SocketId)) return;
            var peer = Find(data.RemoteUserId);
            if (!service.CheckCurrentMembership(data.RemoteUserId) || (!server && data.RemoteUserId != service.HostId)
                || (server && peer == null && peers.Count >= MatchFormat.Duel.TotalPlayers - 1))
            { CloseNative(data.RemoteUserId); return; }
            var accept = new AcceptConnectionOptions { LocalUserId = service.UserId, RemoteUserId = data.RemoteUserId, SocketId = socket };
            if (service.P2P.AcceptConnection(ref accept) != Result.Success) { CloseNative(data.RemoteUserId); return; }
            if (peer == null && server) peers[nextPeer] = new Peer { Id = nextPeer++, User = data.RemoteUserId };
        }
        private void Established(ref OnPeerConnectionEstablishedInfo data)
        {
            if (!running || !SameSocket(data.SocketId) || !service.CheckCurrentMembership(data.RemoteUserId)) return;
            var peer = Find(data.RemoteUserId);
            if (peer == null || peer.Connected) return;
            peer.Connected = true;
            events.Enqueue(new KeyValuePair<ulong, NetworkEvent>(peer.Id, NetworkEvent.Connect));
            SendHello(peer.User);
        }
        private void Closed(ref OnRemoteConnectionClosedInfo data)
        {
            if (!running || !SameSocket(data.SocketId)) return;
            var peer = Find(data.RemoteUserId);
            if (peer == null) return;
            peers.Remove(peer.Id);
            if (events.Count < 32) events.Enqueue(new KeyValuePair<ulong, NetworkEvent>(peer.Id, NetworkEvent.Disconnect));
        }
        private bool SendHello(ProductUserId target)
        {
            var send = new SendPacketOptions
            {
                LocalUserId = service.UserId, RemoteUserId = target, SocketId = socket, Channel = 0,
                Data = new ArraySegment<byte>(new byte[] { 1 }), Reliability = PacketReliability.ReliableOrdered,
                AllowDelayedDelivery = true, DisableAutoAcceptConnection = true
            };
            return service.P2P.SendPacket(ref send) == Result.Success;
        }
        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = 0; payload = default; receiveTime = Time.realtimeSinceStartup;
            if (events.Count > 0)
            {
                var item = events.Dequeue(); clientId = item.Key; return item.Value;
            }
            if (!running || service.P2P == null || !service.Authenticated) return NetworkEvent.Nothing;
            foreach (var peer in peers.Values)
                if (!service.IsMember(peer.User) || peer.Receiver.HasExpired(Time.realtimeSinceStartupAsDouble))
                { clientId = peer.Id; ClosePeer(clientId); return NetworkEvent.Disconnect; }
            var options = new ReceivePacketOptions { LocalUserId = service.UserId, MaxDataSizeBytes = EosPacket.FrameSize };
            // Bound work per PollEvent; malformed/hello traffic cannot create an unbounded loop.
            for (int i = 0; i < 32; i++)
            {
                ProductUserId sender = null; var receivedSocket = new SocketId();
                Result result = service.P2P.ReceivePacket(ref options, ref sender, ref receivedSocket, out byte channel, new ArraySegment<byte>(incoming), out uint length);
                if (result != Result.Success) break;
                var peer = Find(sender);
                if (peer == null || !peer.Connected || !SameSocket(receivedSocket) || !service.IsMember(sender) || channel == 0) continue;
                if (!peer.Receiver.TryReceive(incoming, (int)length, channel, Time.realtimeSinceStartupAsDouble, out payload)) continue;
                clientId = peer.Id; return NetworkEvent.Data;
            }
            return NetworkEvent.Nothing;
        }
        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery delivery)
        {
            if (!running || !service.Authenticated || !peers.TryGetValue(clientId, out var peer) || !peer.Connected) return;
            byte channel = delivery == NetworkDelivery.Unreliable ? (byte)1 : delivery == NetworkDelivery.UnreliableSequenced ? (byte)2
                : delivery == NetworkDelivery.Reliable ? (byte)3 : (byte)4;
            try
            {
                foreach (var frame in EosPacket.Encode(payload, channel, peer.MessageId++))
                {
                    var send = new SendPacketOptions
                    {
                        LocalUserId = service.UserId, RemoteUserId = peer.User, SocketId = socket, Channel = channel,
                        Data = new ArraySegment<byte>(frame), AllowDelayedDelivery = true, DisableAutoAcceptConnection = true,
                        Reliability = channel <= 2 ? PacketReliability.UnreliableUnordered : channel == 3 ? PacketReliability.ReliableUnordered : PacketReliability.ReliableOrdered
                    };
                    if (service.P2P.SendPacket(ref send) != Result.Success) throw new InvalidOperationException("EOS send failed.");
                }
            }
            catch
            {
                // Dropping a reliable fragment violates NGO's contract. Fail the connection instead.
                Debug.LogWarning("TD_EOS_SEND_FAILED bounded-packet-or-queue");
                ClosePeer(clientId);
                if (events.Count < 32) events.Enqueue(new KeyValuePair<ulong, NetworkEvent>(clientId, NetworkEvent.Disconnect));
            }
        }
        public override ulong GetCurrentRtt(ulong clientId) => 0; // No measured RTT HUD; not a latency guarantee.
        public override void DisconnectLocalClient() => ClosePeer(0);
        public override void DisconnectRemoteClient(ulong clientId) => ClosePeer(clientId);
        private void CloseNative(ProductUserId user)
        {
            if (service?.P2P == null || service.UserId == null) return;
            var close = new CloseConnectionOptions { LocalUserId = service.UserId, RemoteUserId = user, SocketId = socket };
            service.P2P.CloseConnection(ref close);
        }
        private void ClosePeer(ulong id)
        {
            if (!peers.TryGetValue(id, out var peer)) return;
            peers.Remove(id); CloseNative(peer.User);
        }
        public override void Shutdown()
        {
            running = false;
            if (service?.P2P != null)
            {
                foreach (var peer in peers.Values) CloseNative(peer.User);
                if (incomingNotification != 0) service.P2P.RemoveNotifyPeerConnectionRequest(incomingNotification);
                if (establishedNotification != 0) service.P2P.RemoveNotifyPeerConnectionEstablished(establishedNotification);
                if (closedNotification != 0) service.P2P.RemoveNotifyPeerConnectionClosed(closedNotification);
                if (service.UserId != null)
                {
                    var clear = new ClearPacketQueueOptions { LocalUserId = service.UserId, SocketId = socket };
                    service.P2P.ClearPacketQueue(ref clear);
                }
            }
            peers.Clear(); events.Clear(); incomingNotification = establishedNotification = closedNotification = 0;
        }
    }
}
