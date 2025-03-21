namespace SystemB.Models;

public class TokenExchangeRequest
{
    public string Token { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
} 