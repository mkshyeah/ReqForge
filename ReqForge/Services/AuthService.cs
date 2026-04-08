using System.Security.Cryptography;
using System.Text;
using ReqForge.Data;
using ReqForge.Models;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private const int Pbkdf2Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Pbkdf2Prefix = "pbkdf2";

    public bool IsLoggedIn { get; private set; }
    public string? CurrentUsername { get; private set; }

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        if (_db.Users.Any(u => u.UserName == username))
            return false;

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashPasswordPbkdf2(password, saltBytes, Pbkdf2Iterations);
        var storedHash = $"{Pbkdf2Prefix}${Pbkdf2Iterations}${salt}${hash}";

        var newUser = new User
        {
            UserName = username,
            PasswordHash = storedHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Users.Add(newUser);
        _db.SaveChanges();

        IsLoggedIn = true;
        CurrentUsername = username;
        return true;
    }

    public bool Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null) return false;

        var isValid = VerifyPassword(password, user);
        if (!isValid) return false;

        IsLoggedIn = true;
        CurrentUsername = username;
        return true;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        CurrentUsername = null;
    }

    private bool VerifyPassword(string password, User user)
    {
        if (TryVerifyPbkdf2(password, user.PasswordHash))
            return true;

        // Backward-compatible fallback for old SHA256+salt hashes.
        var legacyHash = HashPasswordLegacy(password, user.Salt);
        if (!FixedEqualsBase64(user.PasswordHash, legacyHash))
            return false;

        // Upgrade hash on successful legacy login.
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashPasswordPbkdf2(password, saltBytes, Pbkdf2Iterations);
        user.Salt = salt;
        user.PasswordHash = $"{Pbkdf2Prefix}${Pbkdf2Iterations}${salt}${hash}";
        _db.SaveChanges();
        return true;
    }

    private static bool TryVerifyPbkdf2(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !parts[0].Equals(Pbkdf2Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedHash = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string HashPasswordPbkdf2(string password, byte[] saltBytes, int iterations)
    {
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    private static string HashPasswordLegacy(string password, string salt)
    {
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static bool FixedEqualsBase64(string value1, string value2)
    {
        try
        {
            var bytes1 = Convert.FromBase64String(value1);
            var bytes2 = Convert.FromBase64String(value2);
            return CryptographicOperations.FixedTimeEquals(bytes1, bytes2);
        }
        catch
        {
            return false;
        }
    }
}
