using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ReqForge.Data;
using ReqForge.Models;

namespace ReqForge.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    
    // Свойства для хранения состояния текущей сессии
    public bool IsLoggedIn { get; private set; }
    public string? CurrentUsername { get; private set; }

    public AuthService(AppDbContext db)
    {
        _db = db;
    }
    
    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        
        if (_db.Users.Any(u => u.UserName == username))
            return false;
        
        // 1. Генерация соли (случайные 16 байт)
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        
        // 2. Хеширование пароля с этой солью
        var hash = HashPassword(password, salt);
        
        // 3. Создание и сохранение пользователя
        var newUser = new User
        {
            UserName = username,
            PasswordHash = hash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
        };
        
        _db.Users.Add(newUser);
        _db.SaveChanges();
        
        // 4. Автоматический вход после регистрации
        IsLoggedIn = true;
        CurrentUsername = username;
        return true;
    }

    public bool Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserName==username);

        if (user == null) return false;
        
        var inputHash = HashPassword(password, user.Salt);

        if (inputHash == user.PasswordHash)
        {
            IsLoggedIn = true;
            CurrentUsername = username;
            return true;
        }
        
        return false;
    }

    
    public void Logout()
    {
        IsLoggedIn = false;
        CurrentUsername = null;
    }
    
    private string HashPassword(string password, string salt)
    {
        // Соединяем пароль и соль, переводим в байты
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        // Вычисляем SHA256 хеш
        var hashBytes = SHA256.HashData(bytes);
        // Возвращаем как строку Base64
        return Convert.ToBase64String(hashBytes);
    }
}