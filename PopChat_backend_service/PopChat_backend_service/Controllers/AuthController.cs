using Microsoft.AspNetCore.Mvc;
using PopChat_backend_service.Model;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username))
        {
            return BadRequest("Invalid request: Username is required.");
        }

        var token = _jwtService.GenerateToken(request.Username);
        return Ok(new { token });
    }
}