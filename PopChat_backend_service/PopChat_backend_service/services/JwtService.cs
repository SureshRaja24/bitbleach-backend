using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Ensure this namespace is included

public class JwtService
{
    private readonly string _secretKey;

    // Inject IConfiguration to read the key from the environment
    public JwtService(IConfiguration configuration)
    {
        // This looks for "JwtSettings:SecretKey" in appsettings.json or Environment Variables
        _secretKey = configuration["JwtSettings:SecretKey"]
                     ?? throw new ArgumentNullException("JWT Secret Key is not configured.");
    }

    public string GenerateToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(10), // Strict 10-minute expiry
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Still available for internal logic if needed
    public string GetSecretKey() => _secretKey;
}