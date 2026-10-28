using API.Application.Messaging;

namespace API.Application.Features.Tasks.Commands.FinalDecision;

public sealed record FinalDecisionCommand(Guid TicketId, FinalDecisionRequest Request) : ICommand<bool>;
