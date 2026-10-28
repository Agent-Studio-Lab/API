namespace API.Application.Abstractions;

public interface ILLMProviderFactory
{
    ILLMProvider GetProvider(string providerName);
}
