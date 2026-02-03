using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Ncp.Admin.Infrastructure.Services;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 基于 PBKDF2 的密码哈希实现
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        return $"{salt}.{hash}";
    }

    public bool Verify(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword));

        var parts = hashedPassword.Split('.');
        if (parts.Length != 2)
            return false;
        return HashPassword(password, parts[0]) == parts[1];
    }

    private static string HashPassword(string value, string salt)
    {
        var valueBytes = KeyDerivation.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: 100000,
            numBytesRequested: 256 / 8);
        return Convert.ToBase64String(valueBytes);
    }

    private static string GenerateSalt()
    {
        var randomBytes = new byte[128 / 8];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
