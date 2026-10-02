using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace TowerDefense.Networking
{
    public readonly struct RelayConnection
    {
        public readonly string JoinCode;
        public readonly RelayServerData ServerData;
        public RelayConnection(string code, RelayServerData data) { JoinCode = code; ServerData = data; }
    }

    public interface IRelayConnector
    {
        Task<RelayConnection> HostAsync();
        Task<RelayConnection> JoinAsync(string code);
    }

    public sealed class RelayConnector : IRelayConnector, IDisposable
    {
        private Mutex profileLease;
        private string profileName;
        private Task initialization;

        public async Task<RelayConnection> HostAsync()
        {
            await InitializeAsync();
            var allocation = await RelayService.Instance.CreateAllocationAsync(1); // One guest plus the host.
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return new RelayConnection(code, AllocationUtils.ToRelayServerData(allocation, "dtls"));
        }

        public async Task<RelayConnection> JoinAsync(string code)
        {
            await InitializeAsync();
            var allocation = await RelayService.Instance.JoinAllocationAsync(code);
            return new RelayConnection(code, AllocationUtils.ToRelayServerData(allocation, "dtls"));
        }

        private Task InitializeAsync()
        {
            if (initialization == null || initialization.IsFaulted || initialization.IsCanceled)
                initialization = InitializeServicesAsync();
            return initialization;
        }

        private async Task InitializeServicesAsync()
        {
            if (string.IsNullOrEmpty(Application.cloudProjectId))
                throw new InvalidOperationException("A linked Unity cloud project is required.");
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions().SetProfile(AcquireProfile());
                await UnityServices.InitializeAsync(options);
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private string AcquireProfile()
        {
            if (profileLease != null) return profileName;
            // Stable cached guest identities, with a different slot for simultaneous copies on one PC.
            // The operating system releases a lease if a process crashes. No credentials enter game state.
            for (int slot = 0; slot < 16; slot++)
            {
                var lease = new Mutex(false, "TowerDefenseUGS-" + Application.cloudProjectId + "-" + slot);
                bool acquired;
                try { acquired = lease.WaitOne(0); }
                catch (AbandonedMutexException) { acquired = true; }
                if (acquired) { profileLease = lease; profileName = "td-guest-" + slot; return profileName; }
                lease.Dispose();
            }
            throw new InvalidOperationException("Too many game copies are using online play on this PC.");
        }

        public void Dispose()
        {
            if (profileLease == null) return;
            profileLease.ReleaseMutex();
            profileLease.Dispose();
            profileLease = null;
        }

        public static string DescribeFailure(Exception error)
        {
            if (error is TimeoutException) return "Online connection timed out. Check your internet connection and try again.";
            if (error is RelayServiceException relay)
            {
                switch (relay.Reason)
                {
                    case RelayExceptionReason.JoinCodeNotFound:
                    case RelayExceptionReason.AllocationNotFound:
                    case RelayExceptionReason.EntityNotFound:
                        return "That room code is invalid or has expired. Ask the host for a new code.";
                    case RelayExceptionReason.InactiveProject:
                    case RelayExceptionReason.Forbidden:
                        return "Online play is unavailable for this game project. The owner needs to enable Unity Relay.";
                    case RelayExceptionReason.PaymentRequired:
                        return "The game's online service limit has been reached. Try LAN play or contact the owner.";
                    case RelayExceptionReason.RateLimited:
                        return "Too many connection attempts. Wait a moment before trying again.";
                    case RelayExceptionReason.Conflict:
                        return "This room is full or unavailable. Ask the host to create a new room.";
                }
            }
            return "Could not connect online. Check your internet connection and the room code, then try again.";
        }
    }
}
