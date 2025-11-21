using System;
using System.Security.Cryptography;
using System.Text;
using Espectaculos.Application.Abstractions.Security;

namespace Espectaculos.Infrastructure.Security
{
    /// <summary>
    /// Implementación PBKDF2.
    /// Formato del hash: {iterations}.{saltBase64}.{hashBase64}
    /// </summary>
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize   = 16;      // 128 bits
        private const int KeySize    = 32;      // 256 bits
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                return false;

            var parts = passwordHash.Split('.', 3);
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            var testHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                Algorithm,
                hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
    }
}