using System.Security.Cryptography;
using System.Text;

namespace SystemB.Services;

public class SignatureService
{
    private readonly IConfiguration _configuration;
    private readonly string _signingKey;
    private readonly ILogger<SignatureService> _logger;

    public SignatureService(IConfiguration configuration, ILogger<SignatureService> logger)
    {
        _configuration = configuration;
        _signingKey = _configuration["Signature:Key"] ?? throw new ArgumentNullException("Signature:Key");
        _logger = logger;
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
        
        _logger.LogInformation("验证签名: SystemId={SystemId}, 受信任系统={TrustedSystems}", 
            systemId, string.Join(", ", trustedSystems));
        
        // 检查SystemId是否在受信任的系统列表中
        if (!trustedSystems.Contains(systemId))
        {
            _logger.LogWarning("系统 {SystemId} 不在受信任列表中", systemId);
            return false;
        }
        
        string expectedSignature = GenerateSignature(token, systemId);
        _logger.LogInformation("签名比较: 接收={ReceivedSignature}, 预期={ExpectedSignature}", 
            signature, expectedSignature);
            
        bool isValid = signature.Equals(expectedSignature);
        _logger.LogInformation("签名验证结果: {Result}", isValid ? "有效" : "无效");
        
        return isValid;
    }
} 