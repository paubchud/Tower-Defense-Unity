using System;
using System.Threading.Tasks;
using TowerDefense.Core;
using Unity.Netcode;

namespace TowerDefense.Networking
{
    public readonly struct OnlineConnection
    {
        public readonly string JoinCode;
        public readonly NetworkTransport Transport;
        public readonly AccountIdentity Account;
        public OnlineConnection(string code, NetworkTransport transport, AccountIdentity account)
        { JoinCode = code; Transport = transport; Account = account; }
    }

    // Provider owns identity, rooms, code validation, transport setup and platform-specific errors.
    // Match rules remain unaware of Steam. A guest provider can implement this later.
    public interface IOnlineProvider
    {
        string DisplayName { get; }
        string PendingInvite { get; }
        bool SupportsFriendInvites { get; }
        event Action HostLost;
        bool TryNormalizeCode(string input, out string code);
        Task<OnlineConnection> PrepareAsync(bool host, string code);
        void Leave();
        bool ShowInviteOverlay();
        string DescribeFailure(Exception error);
    }

    public sealed class SteamOnlineProvider : IOnlineProvider
    {
        private readonly SteamLobbyService service;
        private readonly SteamP2PTransport transport;
        public SteamOnlineProvider(SteamLobbyService service, SteamP2PTransport transport)
        { this.service = service; this.transport = transport; }
        public string DisplayName => "Steam";
        public string PendingInvite => service.PendingInvite;
        public bool SupportsFriendInvites => true;
        public event Action HostLost { add => service.HostLost += value; remove => service.HostLost -= value; }
        public bool TryNormalizeCode(string input, out string code) => SteamLobbyCode.TryNormalize(input, out code);
        public async Task<OnlineConnection> PrepareAsync(bool host, string code)
        {
            var room = await (host ? service.HostAsync() : service.JoinAsync(code));
            transport.HostSteamId = room.HostSteamId;
            return new OnlineConnection(room.JoinCode, transport, service.Account);
        }
        public void Leave() => service.Leave();
        public bool ShowInviteOverlay() => service.ShowInviteOverlay();
        public string DescribeFailure(Exception error) => SteamLobbyService.DescribeFailure(error);
    }

    // Explicitly unavailable, not a fake EOS login/room or a hidden Steam dependency.
    // Replace this adapter when the EOS SDK, local credentials and peer checks exist.
    public sealed class UnconfiguredGuestProvider : IOnlineProvider
    {
        public const string SetupMessage = "Your local guest profile is ready. EOS guest internet matchmaking is not connected yet; LAN / This PC is available for testing.";
        public string DisplayName => "Guest";
        public string PendingInvite => string.Empty;
        public bool SupportsFriendInvites => false;
        public event Action HostLost { add { } remove { } }
        public bool TryNormalizeCode(string input, out string code) { code = string.Empty; return false; }
        public Task<OnlineConnection> PrepareAsync(bool host, string code)
            => Task.FromException<OnlineConnection>(new InvalidOperationException(SetupMessage));
        public void Leave() { }
        public bool ShowInviteOverlay() => false;
        public string DescribeFailure(Exception error) => SetupMessage;
    }
}
