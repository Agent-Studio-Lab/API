using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Application.Abstractions;

public interface ILLMProvider
{
    LLMProviderType ProviderType { get; }
    Task<string> GenerateResponseAsync(ModelConfig config, string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
