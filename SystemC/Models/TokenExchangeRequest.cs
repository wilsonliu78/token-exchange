namespace SystemC.Models;

public class TokenExchangeRequest
{
    public string Token { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    // SystemC没有实现签名字段
} 