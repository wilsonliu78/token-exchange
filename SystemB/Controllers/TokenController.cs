using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemB.Models;
using SystemB.Services;
using System.Text.Json;

namespace SystemB.Controllers;

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
        var token = _tokenService.GenerateToken("user123", "SystemB");
        return Ok(new { token });
    }

    [HttpPost("exchange")]
    [AllowAnonymous]
    public IActionResult ExchangeToken([FromBody] TokenExchangeRequest request)
    {
        _logger.LogInformation("收到令牌交换请求: {Request}", JsonSerializer.Serialize(request));
        
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SystemId) || string.IsNullOrEmpty(request.Signature))
        {
            _logger.LogWarning("缺少必要的字段: Token={Token}, SystemId={SystemId}, Signature={Signature}", 
                request.Token?.Length > 0, request.SystemId?.Length > 0, request.Signature?.Length > 0);
            return BadRequest("Token, SystemId, and Signature are required");
        }

        // 验证签名
        _logger.LogInformation("验证签名: Token={TokenLength}, SystemId={SystemId}, Signature={SignatureLength}", 
            request.Token.Length, request.SystemId, request.Signature.Length);
            
        if (!_signatureService.VerifySignature(request.Token, request.SystemId, request.Signature))
        {
            _logger.LogWarning("无效的签名或不被信任的系统: {SystemId}", request.SystemId);
            return Unauthorized("Invalid signature or untrusted system");
        }

        // 验证传入的令牌，但不验证颁发者和接收者
        if (!_tokenService.ValidateExternalToken(request.Token, out var claimsPrincipal))
        {
            _logger.LogWarning("无效的令牌");
            return Unauthorized("Invalid token");
        }

        // 获取用户ID
        var userId = claimsPrincipal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("令牌中未找到用户ID");
            return BadRequest("User ID not found in token");
        }

        // 生成新的令牌
        var newToken = _tokenService.GenerateToken(userId, "SystemB");
        _logger.LogInformation("生成新令牌成功: UserId={UserId}", userId);

        return Ok(new { token = newToken });
    }
}