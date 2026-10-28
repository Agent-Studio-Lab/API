using API.Domain.Common;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Application.Abstractions;

public interface ILLMProvider
{
    LLMProviderType ProviderType { get; }
    Task<string> GenerateResponseAsync(ModelConfig config, string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
    Task<Result<bool>> ValidateConnectionAsync(ModelConfig config, CancellationToken cancellationToken = default);
}
