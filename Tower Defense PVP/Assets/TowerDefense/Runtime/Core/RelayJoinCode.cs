namespace TowerDefense.Core
{
    public static class RelayJoinCode
    {
        // Relay's published code format; never accept addresses or links as room codes.
        private const string Alphabet = "6789BCDFGHJKLMNPQRTW";

        public static bool TryNormalize(string input, out string code)
        {
            code = (input ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length < 6 || code.Length > 12) { code = string.Empty; return false; }
            foreach (char character in code)
                if (Alphabet.IndexOf(character) < 0) { code = string.Empty; return false; }
            return true;
        }
    }
}
