using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemA.Models;
using SystemA.Services;

namespace SystemA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly SignatureService _signatureService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(TokenService tokenService, SignatureService signatureService, ILogger<TokenController> logger)
    {
        _tokenService = tokenService;
        _signatureService = signatureService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public IActionResult GenerateToken()
    {
        var token = _tokenService.GenerateToken("user123", "SystemA");
        return Ok(new { token });
    }

    [HttpPost("exchange")]
    [AllowAnonymous]
    public IActionResult ExchangeToken([FromBody] TokenExchangeRequest request)
    {
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SystemId) || string.IsNullOrEmpty(request.Signature))
        {
            return BadRequest("Token, SystemId, and Signature are required");
        }

        // 验证签名
        if (!_signatureService.VerifySignature(request.Token, request.SystemId, request.Signature))
        {
            _logger.LogWarning("无效的签名或不被信任的系统: {SystemId}", request.SystemId);
            return Unauthorized("Invalid signature or untrusted system");
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
        var newToken = _tokenService.GenerateToken(userId, "SystemA");

        return Ok(new { token = newToken });
    }
}