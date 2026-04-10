using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReqForge.KmgDemo.Auth;
using ReqForge.KmgDemo.DTOs;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Демо-логин: выдаёт JWT для ролей operator / manager.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponseDto> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var username = request.Username.Trim();
        if (!DemoUsers.Users.TryGetValue(username, out var creds))
            return Unauthorized(new { error = "Invalid credentials." });

        if (!string.Equals(creds.Password, request.Password, StringComparison.Ordinal))
            return Unauthorized(new { error = "Invalid credentials." });

        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 120;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, username),
            new(ClaimTypes.Role, creds.Role)
        };

        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponseDto
        {
            Token = tokenString,
            Role = creds.Role,
            ExpiresAt = expires.ToString("O")
        });
    }
}
