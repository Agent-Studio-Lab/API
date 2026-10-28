using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.Events;

namespace API.Application.Features.Tasks.Commands.ExecuteTask;

public sealed class ExecuteTaskHandler : ICommandHandler<ExecuteTaskCommand, ExecuteTaskResponse>
{
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskTicketRepository _taskTicketRepository;
    private readonly IOfficeEventPublisher _eventPublisher;

    public ExecuteTaskHandler(
        IAgentRepository agentRepository,
        ITaskTicketRepository taskTicketRepository,
        IOfficeEventPublisher eventPublisher)
    {
        _agentRepository = agentRepository;
        _taskTicketRepository = taskTicketRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ExecuteTaskResponse>> Handle(ExecuteTaskCommand command, CancellationToken cancellationToken)
    {
        var prompt = command.Request.Prompt?.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result<ExecuteTaskResponse>.Failure(400, "Đề bài không được để trống!");
        }

        var agents = await _agentRepository.GetAllAsync(cancellationToken);
        if (agents.Count == 0)
        {
            return Result<ExecuteTaskResponse>.Failure(400, "Công ty hiện tại chưa có nhân sự nào trong văn phòng.");
        }

        var title = prompt.Length > 50 ? prompt[..47] + "..." : prompt;
        var ticket = new TaskTicket(
            title,
            prompt,
            command.Request.ExecutionMode,
            command.Request.MaxFeedbackLoops);

        ticket.StartDebate();
        await _taskTicketRepository.AddAsync(ticket, cancellationToken);

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreatePhaseChanged(TaskPhase.Debating, $"Đã tiếp nhận nhiệm vụ: '{title}'. Toàn thể nhân sự văn phòng bắt đầu phiên họp bàn tròn tranh luận!"),
            cancellationToken);

        return Result<ExecuteTaskResponse>.Success(new ExecuteTaskResponse(
            ticket.Id,
            ticket.Title,
            ticket.CurrentPhase.ToString(),
            "Đã khởi tạo nhiệm vụ thành công và bắt đầu phiên họp bàn tròn đa mô hình!"
        ));
    }
}
