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
            var systemAResult = await systemAResponse.Content.ReadFromJsonAsync<TokenResponse>();
            Console.WriteLine($"System A Token: {systemAResult?.Token}\n");

            // 2. 将 System A 的令牌交换为 System B 的令牌
            Console.WriteLine("2. 将 System A 的令牌交换为 System B 的令牌...");
            var exchangeRequest = new TokenExchangeRequest
            {
                Token = systemAResult?.Token ?? "",
                SystemId = "SystemA"
            };

            var systemBResponse = await httpClient.PostAsJsonAsync(
                "http://localhost:5002/api/token/exchange",
                exchangeRequest);
            systemBResponse.EnsureSuccessStatusCode();
            var systemBResult = await systemBResponse.Content.ReadFromJsonAsync<TokenResponse>();
            Console.WriteLine($"System B Token: {systemBResult?.Token}\n");

            // 3. 将 System B 的令牌交换回 System A 的令牌
            Console.WriteLine("3. 将 System B 的令牌交换回 System A 的令牌...");
            exchangeRequest = new TokenExchangeRequest
            {
                Token = systemBResult?.Token ?? "",
                SystemId = "SystemB"
            };

            var systemAExchangeResponse = await httpClient.PostAsJsonAsync(
                "http://localhost:5001/api/token/exchange",
                exchangeRequest);
            systemAExchangeResponse.EnsureSuccessStatusCode();
            var systemAExchangeResult = await systemAExchangeResponse.Content.ReadFromJsonAsync<TokenResponse>();
            Console.WriteLine($"System A Exchanged Token: {systemAExchangeResult?.Token}\n");

            Console.WriteLine("令牌交换测试完成！");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"错误: 无法连接到服务器。请确保 SystemA 和 SystemB 都在运行。\n详细信息: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误: {ex.Message}");
        }
    }
}

public class TokenResponse
{
    public string? Token { get; set; }
}

public class TokenExchangeRequest
{
    public string Token { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
}