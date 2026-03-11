using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ReqForge.Models;

namespace ReqForge.Services;

public class AuthService : IAuthService
{
    
    private List<User> _users;
    
    private static readonly string _filePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };
    
    // Свойства для хранения состояния текущей сессии
    public bool IsLoggedIn { get; private set; }
    public string? CurrentUsername { get; private set; }

    public AuthService()
    {
        if (!File.Exists(_filePath))
        {
            _users = new List<User>();
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch
        {
            _users = new List<User>();
        }
    }
    
    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        
        if (_users.Any(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return false;
        
        // 1. Генерация соли (случайные 16 байт)
        var saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
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
        
        _users.Add(newUser);
        SaveUsers();
        
        // 4. Автоматический вход после регистрации
        IsLoggedIn = true;
        CurrentUsername = username;
        return true;
    }

    public bool Login(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));

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

    private void SaveUsers()
    {
        var json = JsonSerializer.Serialize(_users, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}