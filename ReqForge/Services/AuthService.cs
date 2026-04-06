using System.Security.Cryptography;
using System.Text;
using ReqForge.Data;
using ReqForge.Models;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

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

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashPassword(password, salt);

        var newUser = new User
        {
            UserName = username,
            PasswordHash = hash,
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

        var inputHash = HashPassword(password, user.Salt);
        if (inputHash != user.PasswordHash) return false;

        IsLoggedIn = true;
        CurrentUsername = username;
        return true;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        CurrentUsername = null;
    }

    private static string HashPassword(string password, string salt)
    {
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
