using System.Security.Cryptography;
using System.Text;

namespace SystemB.Services;

public class SignatureService
{
    private readonly IConfiguration _configuration;
    private readonly string _signingKey;

    public SignatureService(IConfiguration configuration)
    {
        _configuration = configuration;
        _signingKey = _configuration["Signature:Key"] ?? throw new ArgumentNullException("Signature:Key");
    }

    public string GenerateSignature(string token, string systemId)
    {
        string dataToSign = $"{token}:{systemId}";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingKey));
        byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
        
        return Convert.ToBase64String(signatureBytes);
    }

    public bool VerifySignature(string token, string systemId, string signature)
    {
        // 获取需要检查的系统列表
        var trustedSystems = _configuration.GetSection("TrustedSystems").Get<List<string>>() ?? new List<string>();
        
        // 检查SystemId是否在受信任的系统列表中
        if (!trustedSystems.Contains(systemId))
        {
            return false;
        }
        
        string expectedSignature = GenerateSignature(token, systemId);
        return signature.Equals(expectedSignature);
    }
} 