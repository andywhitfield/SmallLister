using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace SmallLister.Security
{
    public static class SaltHashHelper
    {
        public static (string Salt, string Hash) CreateHash(string password)
        {
            var salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            var saltString = Convert.ToBase64String(salt);
            var hashString = Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 10_000, 256 / 8));
            return (saltString, hashString);
        }
        public static string CreateHash(string password, string saltString) =>
            Convert.ToBase64String(KeyDerivation.Pbkdf2(password, Convert.FromBase64String(saltString), KeyDerivationPrf.HMACSHA512, 10_000, 256 / 8));
    }
}