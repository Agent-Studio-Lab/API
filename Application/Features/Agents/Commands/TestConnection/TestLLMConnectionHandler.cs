using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.ValueObjects;

namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed class TestLLMConnectionHandler : ICommandHandler<TestLLMConnectionCommand, bool>
{
    private readonly ILLMProviderFactory _llmProviderFactory;

    public TestLLMConnectionHandler(ILLMProviderFactory llmProviderFactory)
    {
        _llmProviderFactory = llmProviderFactory;
    }

    public async Task<Result<bool>> Handle(TestLLMConnectionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var config = new ModelConfig(
            request.Provider,
            request.ModelName,
            request.ApiKey,
            request.BaseUrl);

        var provider = _llmProviderFactory.GetProvider(request.Provider);
        return await provider.ValidateConnectionAsync(config, cancellationToken);
    }
}
