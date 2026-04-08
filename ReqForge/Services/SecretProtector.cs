using System.Security.Cryptography;
using System.Text;

namespace ReqForge.Services;

internal static class SecretProtector
{
    private const string Prefix = "enc:v1:";

    public static string Protect(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(protectedBytes);
    }

    public static string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (!value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        var encoded = value[Prefix.Length..];
        try
        {
            var protectedBytes = Convert.FromBase64String(encoded);
            var plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
