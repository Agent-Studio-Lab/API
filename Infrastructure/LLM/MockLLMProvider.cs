using API.Application.Abstractions;
using API.Domain.Common;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class MockLLMProvider : ILLMProvider
{
    public string ProviderName { get; }

    public MockLLMProvider(string providerName = "Mock")
    {
        ProviderName = string.IsNullOrWhiteSpace(providerName) ? "Mock" : providerName;
    }

    public Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        string response;

        if (userPrompt.Contains("RecommendedModel", StringComparison.OrdinalIgnoreCase) ||
            userPrompt.Contains("Phòng Tuyển dụng", StringComparison.OrdinalIgnoreCase))
        {
            response = """
                {
                  "RecommendedProvider": "DeepSeek",
                  "RecommendedModel": "deepseek-reasoner",
                  "ModelBenchmarkRationale": "Vượt trội về khả năng suy luận logic chuyên sâu với chi phí tối ưu",
                  "DraftSystemPrompt": "Bạn là chuyên gia được tuyển chọn để giải quyết bài toán chuyên môn."
                }
                """;
        }
        else if (userPrompt.Contains("vote", StringComparison.OrdinalIgnoreCase) ||
                 userPrompt.Contains("bỏ phiếu", StringComparison.OrdinalIgnoreCase))
        {
            response = """
                {
                  "ProposedPrimaryExecutor": "Athena",
                  "ProposedTester": "Valeria",
                  "NeedsHiring": false,
                  "MissingRole": null,
                  "MissingSkill": null,
                  "Reasoning": "Yêu cầu có thể được phân tích và kiểm soát chất lượng bởi đội ngũ hạt nhân hiện có."
                }
                """;
        }
        else
        {
            response = $"[Mô phỏng phản hồi từ {config.Provider} - {config.ModelName}]: Tôi đã xử lý yêu cầu dựa trên vai trò chuyên môn.";
        }

        return Task.FromResult(response);
    }

    public Task<Result<bool>> ValidateConnectionAsync(
        ModelConfig config,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
