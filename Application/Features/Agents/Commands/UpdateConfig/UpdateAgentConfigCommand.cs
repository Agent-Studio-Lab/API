using API.Application.Messaging;

namespace API.Application.Features.Agents.Commands.UpdateConfig;

public sealed record UpdateAgentConfigCommand(UpdateAgentConfigRequest Request) : ICommand<bool>;
