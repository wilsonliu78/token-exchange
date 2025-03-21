using System.Net.Http.Json;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("开始令牌交换测试...\n");

        var httpClient = new HttpClient();

        try
        {
            // 1. 从 System A 获取令牌
            Console.WriteLine("1. 从 System A 获取令牌...");
            var systemAResponse = await httpClient.PostAsync("http://localhost:5001/api/token/generate", null);
            systemAResponse.EnsureSuccessStatusCode();
            var systemAContent = await systemAResponse.Content.ReadFromJsonAsync<TokenResponse>();
            string tokenFromA = systemAContent!.Token;
            Console.WriteLine($"System A 令牌: {tokenFromA}\n");

            // 2. 获取对SystemB的签名
            Console.WriteLine("2. 从 System A 获取对 System B 的签名...");
            var signatureRequestA = new TokenExchangeRequest
            {
                Token = tokenFromA,
                SystemId = "SystemB"
            };
            
            var signatureResponseA = await httpClient.PostAsJsonAsync("http://localhost:5001/api/signature/generate", signatureRequestA);
            signatureResponseA.EnsureSuccessStatusCode();
            var signatureContentA = await signatureResponseA.Content.ReadFromJsonAsync<SignatureResponse>();
            string signatureForB = signatureContentA!.Signature;
            Console.WriteLine($"签名: {signatureForB}\n");

            // 3. 使用 System A 的令牌向 System B 请求交换（带签名）
            Console.WriteLine("3. 将 System A 的令牌交换为 System B 的令牌（带签名）...");
            var exchangeRequestB = new TokenExchangeRequest
            {
                Token = tokenFromA,
                SystemId = "SystemA",
                Signature = signatureForB
            };
            
            Console.WriteLine($"请求详情: {JsonSerializer.Serialize(exchangeRequestB)}");
            
            var systemBResponse = await httpClient.PostAsJsonAsync("http://localhost:5002/api/token/exchange", exchangeRequestB);
            systemBResponse.EnsureSuccessStatusCode();
            var systemBContent = await systemBResponse.Content.ReadFromJsonAsync<TokenResponse>();
            string tokenFromB = systemBContent!.Token;
            Console.WriteLine($"System B 令牌: {tokenFromB}\n");

            // 4. 从SystemB获取对SystemA的签名
            Console.WriteLine("4. 从 System B 获取对 System A 的签名...");
            var signatureRequestB = new TokenExchangeRequest
            {
                Token = tokenFromB,
                SystemId = "SystemA"
            };
            
            var signatureResponseB = await httpClient.PostAsJsonAsync("http://localhost:5002/api/signature/generate", signatureRequestB);
            signatureResponseB.EnsureSuccessStatusCode();
            var signatureContentB = await signatureResponseB.Content.ReadFromJsonAsync<SignatureResponse>();
            string signatureForA = signatureContentB!.Signature;
            Console.WriteLine($"签名: {signatureForA}\n");

            // 5. 使用 System B 的令牌向 System A 请求交换（带签名）
            Console.WriteLine("5. 将 System B 的令牌交换回 System A 的令牌（带签名）...");
            var exchangeRequestA = new TokenExchangeRequest
            {
                Token = tokenFromB,
                SystemId = "SystemB",
                Signature = signatureForA
            };
            
            var systemAExchangeResponse = await httpClient.PostAsJsonAsync("http://localhost:5001/api/token/exchange", exchangeRequestA);
            systemAExchangeResponse.EnsureSuccessStatusCode();
            var systemAExchangeContent = await systemAExchangeResponse.Content.ReadFromJsonAsync<TokenResponse>();
            string tokenFromAWithB = systemAExchangeContent!.Token;
            Console.WriteLine($"System A 新令牌: {tokenFromAWithB}\n");

            // 6. 从 System C 获取令牌
            Console.WriteLine("6. 从 System C 获取令牌...");
            var systemCResponse = await httpClient.PostAsync("http://localhost:5003/api/token/generate", null);
            systemCResponse.EnsureSuccessStatusCode();
            var systemCContent = await systemCResponse.Content.ReadFromJsonAsync<TokenResponse>();
            string tokenFromC = systemCContent!.Token;
            Console.WriteLine($"System C 令牌: {tokenFromC}\n");

            // 7. 使用 System C 的令牌向 System A 请求交换（没有签名）
            Console.WriteLine("7. 尝试将 System C 的令牌交换到 System A（无签名，预期失败）...");
            var exchangeRequestCToA = new TokenExchangeRequest
            {
                Token = tokenFromC,
                SystemId = "SystemC"
                // 没有提供签名
            };
            
            try 
            {
                var systemAResponseWithC = await httpClient.PostAsJsonAsync("http://localhost:5001/api/token/exchange", exchangeRequestCToA);
                
                if (systemAResponseWithC.IsSuccessStatusCode)
                {
                    Console.WriteLine("错误：System A 不应接受没有签名的请求!");
                }
                else
                {
                    Console.WriteLine($"预期结果：System A 拒绝了 System C 的请求（状态码: {systemAResponseWithC.StatusCode}）");
                    Console.WriteLine($"错误信息: {await systemAResponseWithC.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"预期结果：请求失败: {ex.Message}");
            }
            
            Console.WriteLine("\n测试完成。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
    }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
}

public class SignatureResponse
{
    public string Signature { get; set; } = string.Empty;
}

public class TokenExchangeRequest
{
    public string Token { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}