using System.Collections;
using System.IO;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Networking
{
    public sealed class PlayerSignInService : MonoBehaviour
    {
        public StartupSignIn Flow { get; } = new StartupSignIn();
        public bool Ready => Flow.State == StartupSignInState.SignedIn;
        public AccountIdentity Account => Flow.Account;
        private SteamLobbyService steam;

        public void Initialize(SteamLobbyService service)
        {
            steam = service;
            RetrySteam();
        }

        public void RetrySteam()
        {
            if (Ready || steam == null) return;
            int attempt = Flow.BeginSteamCheck();
            StartCoroutine(CheckSteam(attempt));
        }

        private IEnumerator CheckSteam(int attempt)
        {
            yield return null; // Render the startup screen before asking the native SDK.
            if (steam.TrySignIn(out var account, out var name, out var failure))
                Flow.FinishSteamCheck(attempt, account, name, string.Empty);
            else Flow.FinishSteamCheck(attempt, default, string.Empty, failure);
        }

        public bool ContinueAsGuest() => Flow.ContinueAsGuest(() =>
            new GuestProfileStore(Path.Combine(Application.persistentDataPath, "Accounts")).LoadOrCreate());
    }
}
