using API.Application.Abstractions;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class MockLLMProvider : ILLMProvider
{
    public LLMProviderType ProviderType { get; }

    public MockLLMProvider(LLMProviderType providerType = LLMProviderType.OpenAI)
    {
        ProviderType = providerType;
    }

    public Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var response = userPrompt.Contains("vote", StringComparison.OrdinalIgnoreCase) ||
                       userPrompt.Contains("bỏ phiếu", StringComparison.OrdinalIgnoreCase)
            ? """
              {
                "ProposedPrimaryExecutor": "Athena",
                "ProposedTester": "Valeria",
                "NeedsHiring": false,
                "MissingRole": null,
                "MissingSkill": null,
                "Reasoning": "Yêu cầu có thể được phân tích và kiểm soát chất lượng bởi đội ngũ hạt nhân hiện có."
              }
              """
            : $"[Mô phỏng phản hồi từ {config.Provider} - {config.ModelName}]: Tôi đã xử lý yêu cầu dựa trên vai trò chuyên môn.";

        return Task.FromResult(response);
    }
}
