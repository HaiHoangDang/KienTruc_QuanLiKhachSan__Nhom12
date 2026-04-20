using System;
using System.Security.Cryptography;

namespace auth_service.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int DefaultIterations = 100000;
        private const string Prefix = "$DKS$PBKDF2$";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password must not be empty.", nameof(password));
            }

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = DeriveHash(password, salt, DefaultIterations);
            return $"{Prefix}{DefaultIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string storedValue, string candidatePassword, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrEmpty(storedValue) || candidatePassword == null)
            {
                return false;
            }

            if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
            {
                var legacyMatched = string.Equals(storedValue, candidatePassword, StringComparison.Ordinal);
                needsRehash = legacyMatched;
                return legacyMatched;
            }

            var parts = storedValue.Split('$');
            if (parts.Length != 6)
            {
                return false;
            }

            if (!int.TryParse(parts[3], out var iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[4]);
                expectedHash = Convert.FromBase64String(parts[5]);
            }
            catch
            {
                return false;
            }

            var actualHash = DeriveHash(candidatePassword, salt, iterations);
            var matched = FixedTimeEquals(actualHash, expectedHash);
            needsRehash = matched && iterations < DefaultIterations;
            return matched;
        }

        private static byte[] DeriveHash(string password, byte[] salt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
