using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemB.Models;
using SystemB.Services;

namespace SystemB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(TokenService tokenService, ILogger<TokenController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public IActionResult GenerateToken()
    {
        var token = _tokenService.GenerateToken("user123", "SystemB");
        return Ok(new { token });
    }

    [HttpPost("exchange")]
    [AllowAnonymous]
    public IActionResult ExchangeToken([FromBody] TokenExchangeRequest request)
    {
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SystemId))
        {
            return BadRequest("Token and SystemId are required");
        }

        // 验证传入的令牌，但不验证颁发者和接收者
        if (!_tokenService.ValidateExternalToken(request.Token, out var claimsPrincipal))
        {
            return Unauthorized("Invalid token");
        }

        // 获取用户ID
        var userId = claimsPrincipal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID not found in token");
        }

        // 生成新的令牌
        var newToken = _tokenService.GenerateToken(userId, "SystemB");
        return Ok(new { token = newToken });
    }
}