namespace ReqForge.KmgDemo.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
}