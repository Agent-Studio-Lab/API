using API.Application.Messaging;

namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed record TestLLMConnectionCommand(TestLLMConnectionRequest Request) : ICommand<bool>;
