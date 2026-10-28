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
    private readonly ILLMProviderFactory _llmFactory;
    private readonly IOfficeEventPublisher _eventPublisher;

    public ExecuteTaskHandler(
        IAgentRepository agentRepository,
        ITaskTicketRepository taskTicketRepository,
        ILLMProviderFactory llmFactory,
        IOfficeEventPublisher eventPublisher)
    {
        _agentRepository = agentRepository;
        _taskTicketRepository = taskTicketRepository;
        _llmFactory = llmFactory;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ExecuteTaskResponse>> Handle(ExecuteTaskCommand command, CancellationToken cancellationToken)
    {
        var prompt = command.Request.Prompt?.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result<ExecuteTaskResponse>.Failure(400, "Đề bài không được để trống!");
        }

        var alice = await _agentRepository.GetByRoleAsync(AgentRole.ProductManager, cancellationToken);
        var bob = await _agentRepository.GetByRoleAsync(AgentRole.SoftwareEngineer, cancellationToken);
        var charlie = await _agentRepository.GetByRoleAsync(AgentRole.QualityReviewer, cancellationToken);

        if (alice is null || bob is null || charlie is null)
        {
            return Result<ExecuteTaskResponse>.Failure(500, "Không tìm thấy đủ nhân sự cốt lõi (Alice, Bob, Charlie) để thực thi.");
        }

        var title = prompt.Length > 50 ? prompt[..47] + "..." : prompt;
        var ticket = new TaskTicket(title, prompt);
        await _taskTicketRepository.AddAsync(ticket, cancellationToken);

        try
        {
            // === GIAI ĐOẠN 1: ALICE LẬP KẾ HOẠCH (PLANNING) ===
            ticket.ChangePhase(TaskPhase.Planning);
            await _eventPublisher.PublishAsync(OfficeEvent.CreatePhaseChanged(TaskPhase.Planning, "Bắt đầu giai đoạn 1: Phòng Kế hoạch phân tích đề bài"), cancellationToken);

            alice.UpdateStatus("Đang phân tích đề bài...");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateThinking(alice.Name, alice.Avatar, TaskPhase.Planning, "Đang đọc yêu cầu và lên khung bản đặc tả kỹ thuật..."), cancellationToken);

            var aliceProvider = _llmFactory.GetProvider(alice.ModelConfig.Provider);
            var alicePrompt = $"Yêu cầu từ khách hàng/sếp:\n\"{prompt}\"\n\nHãy soạn bản đặc tả kỹ thuật (Specification) chi tiết để bàn giao cho Developer.";
            var specResponse = await aliceProvider.GenerateResponseAsync(alice.ModelConfig, alice.SystemPrompt, alicePrompt, cancellationToken);
            ticket.SetSpecification(specResponse);

            alice.UpdateStatus("Đã hoàn thành bản đặc tả");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateSpeaking(alice.Name, alice.Avatar, bob.Name, TaskPhase.Planning,
                $"Chào anh {bob.Name}, em đã lên xong bản đặc tả kỹ thuật cho dự án. Anh xem qua và bắt đầu triển khai code nhé!",
                new { spec = specResponse }), cancellationToken);

            await Task.Delay(1000, cancellationToken);

            // === GIAI ĐOẠN 2: BOB LẬP TRÌNH (IMPLEMENTATION) ===
            ticket.ChangePhase(TaskPhase.Implementation);
            await _eventPublisher.PublishAsync(OfficeEvent.CreatePhaseChanged(TaskPhase.Implementation, "Bắt đầu giai đoạn 2: Phòng Kỹ thuật viết mã nguồn"), cancellationToken);

            bob.UpdateStatus("Đang thiết kế kiến trúc và viết code...");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateThinking(bob.Name, bob.Avatar, TaskPhase.Implementation, "Đang nghiên cứu đặc tả của Alice và bắt tay vào viết code giải pháp..."), cancellationToken);

            var bobProvider = _llmFactory.GetProvider(bob.ModelConfig.Provider);
            var bobPrompt = $"Đề bài ban đầu:\n\"{prompt}\"\n\nBản đặc tả từ Alice:\n{ticket.Specification}\n\nHãy viết code giải pháp hoàn chỉnh và tối ưu.";
            var codeResponse = await bobProvider.GenerateResponseAsync(bob.ModelConfig, bob.SystemPrompt, bobPrompt, cancellationToken);
            ticket.SetImplementationCode(codeResponse);

            bob.UpdateStatus("Đã hoàn thành mã nguồn");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateSpeaking(bob.Name, bob.Avatar, charlie.Name, TaskPhase.Implementation,
                $"Chào {charlie.Name}, tôi đã hoàn thiện xong toàn bộ mã nguồn theo yêu cầu của Alice. Nhờ bạn rà soát chất lượng giúp nhé!",
                new { code = codeResponse }), cancellationToken);

            await Task.Delay(1000, cancellationToken);

            // === GIAI ĐOẠN 3: CHARLIE KIỂM THỬ & ĐÁNH GIÁ (REVIEW) ===
            ticket.ChangePhase(TaskPhase.Review);
            await _eventPublisher.PublishAsync(OfficeEvent.CreatePhaseChanged(TaskPhase.Review, "Bắt đầu giai đoạn 3: Phòng QA kiểm soát chất lượng"), cancellationToken);

            charlie.UpdateStatus("Đang rà soát và kiểm thử mã nguồn...");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateThinking(charlie.Name, charlie.Avatar, TaskPhase.Review, "Đang kiểm tra các edge case, kiểm soát lỗi và tiêu chuẩn an toàn..."), cancellationToken);

            var charlieProvider = _llmFactory.GetProvider(charlie.ModelConfig.Provider);
            var charliePrompt = $"Bản đặc tả của Alice:\n{ticket.Specification}\n\nMã nguồn của Bob:\n{ticket.ImplementationCode}\n\nHãy đánh giá toàn diện mã nguồn này.";
            var reviewResponse = await charlieProvider.GenerateResponseAsync(charlie.ModelConfig, charlie.SystemPrompt, charliePrompt, cancellationToken);
            ticket.SetReviewSummary(reviewResponse);

            charlie.UpdateStatus("Đã hoàn tất đánh giá");
            await _eventPublisher.PublishAsync(OfficeEvent.CreateSpeaking(charlie.Name, charlie.Avatar, "Sếp (User)", TaskPhase.Review,
                "Báo cáo Sếp: Tôi đã hoàn thành việc kiểm thử và rà soát mã nguồn. Toàn bộ tính năng đã đạt yêu cầu và sẵn sàng bàn giao!",
                new { review = reviewResponse }), cancellationToken);

            // === HOÀN TẤT DỰ ÁN ===
            ticket.ChangePhase(TaskPhase.Completed);

            await _eventPublisher.PublishAsync(OfficeEvent.CreateCompleted(
                "Đội ngũ Agent Studio đã hoàn thành xuất sắc nhiệm vụ!",
                new
                {
                    ticketId = ticket.Id,
                    title = ticket.Title,
                    spec = ticket.Specification,
                    code = ticket.ImplementationCode,
                    review = ticket.ReviewSummary
                }), cancellationToken);
        }
        finally
        {
            alice.UpdateStatus("Sẵn sàng");
            bob.UpdateStatus("Sẵn sàng");
            charlie.UpdateStatus("Sẵn sàng");
        }

        return Result<ExecuteTaskResponse>.Success(new ExecuteTaskResponse(
            ticket.Id,
            ticket.Title,
            ticket.CurrentPhase.ToString(),
            "Đã tiếp nhận và thực thi hoàn tất quy trình Agent Studio!"
        ));
    }
}
