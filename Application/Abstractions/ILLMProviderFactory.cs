using API.Domain.Enums;

namespace API.Application.Abstractions;

public interface ILLMProviderFactory
{
    ILLMProvider GetProvider(LLMProviderType providerType);
}
