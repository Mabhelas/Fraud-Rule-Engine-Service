using Fraud_Rule_Engine_Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _tokenService;

    public AuthController(JwtTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult Token()
    {
        // TODO: validate user credentials
        var token = _tokenService.GenerateToken(
            "demo-user",
            "FraudAnalyst"
        );

        return Ok(new { access_token = token });
    }
}
