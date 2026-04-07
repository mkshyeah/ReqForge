using Microsoft.EntityFrameworkCore;
using ReqForge.Data;
using ReqForge.Services;

namespace ReqForge.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _auth;

    public AuthServiceTests()
    {
        _db = new TestDbContext(Guid.NewGuid().ToString());
        _db.Database.EnsureCreated();
        _auth = new AuthService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    // === Register ===

    [Fact]
    public void Register_ValidCredentials_ReturnsTrue()
    {
        var result = _auth.Register("testuser", "password123");

        Assert.True(result);
    }

    [Fact]
    public void Register_SetsLoggedInState()
    {
        _auth.Register("testuser", "password123");

        Assert.True(_auth.IsLoggedIn);
        Assert.Equal("testuser", _auth.CurrentUsername);
    }

    [Fact]
    public void Register_CreatesUserInDatabase()
    {
        _auth.Register("testuser", "password123");

        var user = _db.Users.FirstOrDefault(u => u.UserName == "testuser");
        Assert.NotNull(user);
        Assert.NotEmpty(user.PasswordHash);
        Assert.NotEmpty(user.Salt);
    }

    [Fact]
    public void Register_PasswordIsHashed_NotStoredPlaintext()
    {
        _auth.Register("testuser", "mypassword");

        var user = _db.Users.First(u => u.UserName == "testuser");
        Assert.NotEqual("mypassword", user.PasswordHash);
    }

    [Fact]
    public void Register_DuplicateUsername_ReturnsFalse()
    {
        _auth.Register("testuser", "password1");

        var auth2 = new AuthService(_db);
        var result = auth2.Register("testuser", "password2");

        Assert.False(result);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("  ", "password")]
    [InlineData("user", "")]
    [InlineData("user", "  ")]
    [InlineData("", "")]
    [InlineData(null, "password")]
    [InlineData("user", null)]
    public void Register_InvalidCredentials_ReturnsFalse(string? username, string? password)
    {
        var result = _auth.Register(username!, password!);

        Assert.False(result);
    }

    [Fact]
    public void Register_TwoUsers_DifferentSalts()
    {
        _auth.Register("user1", "same_password");

        var auth2 = new AuthService(_db);
        auth2.Register("user2", "same_password");

        var user1 = _db.Users.First(u => u.UserName == "user1");
        var user2 = _db.Users.First(u => u.UserName == "user2");

        Assert.NotEqual(user1.Salt, user2.Salt);
        Assert.NotEqual(user1.PasswordHash, user2.PasswordHash);
    }

    // === Login ===

    [Fact]
    public void Login_ValidCredentials_ReturnsTrue()
    {
        _auth.Register("testuser", "password123");
        _auth.Logout();

        var result = _auth.Login("testuser", "password123");

        Assert.True(result);
    }

    [Fact]
    public void Login_SetsLoggedInState()
    {
        _auth.Register("testuser", "password123");
        _auth.Logout();

        _auth.Login("testuser", "password123");

        Assert.True(_auth.IsLoggedIn);
        Assert.Equal("testuser", _auth.CurrentUsername);
    }

    [Fact]
    public void Login_WrongPassword_ReturnsFalse()
    {
        _auth.Register("testuser", "correct_password");
        _auth.Logout();

        var result = _auth.Login("testuser", "wrong_password");

        Assert.False(result);
        Assert.False(_auth.IsLoggedIn);
    }

    [Fact]
    public void Login_NonexistentUser_ReturnsFalse()
    {
        var result = _auth.Login("ghost", "password");

        Assert.False(result);
    }

    // === Logout ===

    [Fact]
    public void Logout_ClearsState()
    {
        _auth.Register("testuser", "password123");
        Assert.True(_auth.IsLoggedIn);

        _auth.Logout();

        Assert.False(_auth.IsLoggedIn);
        Assert.Null(_auth.CurrentUsername);
    }

    private class TestDbContext : AppDbContext
    {
        private readonly string _dbName;

        public TestDbContext(string dbName) => _dbName = dbName;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(_dbName);
        }
    }
}
