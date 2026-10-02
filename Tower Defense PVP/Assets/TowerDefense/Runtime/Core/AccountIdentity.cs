using System;

namespace TowerDefense.Core
{
    // A profile key, not a Netcode client ID or proof of authentication. Persistence comes later.
    public readonly struct AccountIdentity : IEquatable<AccountIdentity>
    {
        public readonly string Provider;
        public readonly string Subject;
        public bool IsValid => !string.IsNullOrEmpty(Provider) && !string.IsNullOrEmpty(Subject);
        public string ProfileKey => IsValid ? Provider + ":" + Subject : string.Empty;

        public AccountIdentity(string provider, string subject)
        {
            if (!ValidPart(provider, 32) || provider != provider.ToLowerInvariant() || !ValidPart(subject, 128))
                throw new ArgumentException("Account identifiers require a lowercase provider and bounded alphanumeric ID.");
            Provider = provider;
            Subject = subject;
        }

        private static bool ValidPart(string value, int maximum)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maximum) return false;
            foreach (char c in value)
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '-' && c != '_') return false;
            return true;
        }

        public bool Equals(AccountIdentity other) => Provider == other.Provider && Subject == other.Subject;
        public override bool Equals(object other) => other is AccountIdentity identity && Equals(identity);
        public override int GetHashCode() => ((Provider?.GetHashCode() ?? 0) * 397) ^ (Subject?.GetHashCode() ?? 0);
        public override string ToString() => ProfileKey;
    }
}
