namespace ReqForge.Services;

public interface IAuthService
{
    bool Register(string username, string password);
    bool Login(string username, string password);
    bool IsLoggedIn { get; }
    string? CurrentUsername { get; }
    void Logout();
}