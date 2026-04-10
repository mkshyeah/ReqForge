namespace ReqForge.KmgDemo.Auth;


public static class DemoUsers
{
    // Логин/пароль для демо: просто и наглядно
    public static readonly Dictionary<string, (string Password, string Role)> Users = new()
    {
        ["operator1"] = ("operator123", "operator"),
        ["manager1"] = ("manager123", "manager")
    };
}