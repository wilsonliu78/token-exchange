using Microsoft.AspNetCore.Mvc;
using SystemB.Models;
using SystemB.Services;

namespace SystemB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignatureController : ControllerBase
{
    private readonly SignatureService _signatureService;

    public SignatureController(SignatureService signatureService)
    {
        _signatureService = signatureService;
    }

    [HttpPost("generate")]
    public IActionResult GenerateSignature([FromBody] TokenExchangeRequest request)
    {
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SystemId))
        {
            return BadRequest("Token and SystemId are required");
        }

        var signature = _signatureService.GenerateSignature(request.Token, request.SystemId);
        return Ok(new { signature });
    }
} 