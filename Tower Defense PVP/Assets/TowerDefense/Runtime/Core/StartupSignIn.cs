using System;

namespace TowerDefense.Core
{
    public enum StartupSignInState { CheckingSteam, GuestChoice, SignedIn }

    // Startup/profile identity survives leaving a match. It is not a network client ID
    // or proof that an online backend has authenticated the player.
    public sealed class StartupSignIn
    {
        public StartupSignInState State { get; private set; } = StartupSignInState.CheckingSteam;
        public AccountIdentity Account { get; private set; }
        public string DisplayName { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Checking Steam...";
        private int attempt;

        public int BeginSteamCheck()
        {
            if (State == StartupSignInState.SignedIn) throw new InvalidOperationException("Restart before changing your signed-in profile.");
            State = StartupSignInState.CheckingSteam;
            Status = "Checking Steam...";
            return ++attempt;
        }

        public bool FinishSteamCheck(int request, AccountIdentity account, string displayName, string failure)
        {
            if (request != attempt || State != StartupSignInState.CheckingSteam) return false;
            if (account.IsValid && (account.Provider == "steam" || account.Provider == "steam-test"))
            {
                Account = account;
                DisplayName = CleanName(displayName);
                State = StartupSignInState.SignedIn;
                Status = "Signed in through Steam.";
            }
            else
            {
                State = StartupSignInState.GuestChoice;
                Status = string.IsNullOrWhiteSpace(failure) ? "Steam sign-in is unavailable. Continue as a guest or retry Steam." : failure;
            }
            return true;
        }

        public bool ContinueAsGuest(Func<AccountIdentity> loadProfile)
        {
            if (State != StartupSignInState.GuestChoice) return false;
            try
            {
                var account = loadProfile();
                if (!account.IsValid || account.Provider != "guest") throw new InvalidOperationException("Invalid guest profile.");
                Account = account;
                DisplayName = "Guest";
                State = StartupSignInState.SignedIn;
                Status = "Using your local guest profile. Online guest matchmaking is not configured yet.";
                return true;
            }
            catch (Exception)
            {
                // Never replace an unreadable profile silently or disclose filesystem paths.
                Status = "Could not load or save your guest profile. Your existing files were kept. Check storage access and retry.";
                return false;
            }
        }

        private static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Steam Player";
            name = name.Trim();
            foreach (char value in name) if (char.IsControl(value)) return "Steam Player";
            return name.Length > 64 ? name.Substring(0, 64) : name;
        }
    }
}
