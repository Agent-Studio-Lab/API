using API.Application.Messaging;

namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed record ApproveHiringCommand(Guid TicketId, ApproveHiringRequest Request) : ICommand<bool>;
