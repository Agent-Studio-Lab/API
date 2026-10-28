using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.Events;
using API.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace API.Infrastructure.BackgroundJobs;

public sealed class TaskWorkflowJob
{
    private readonly ITaskTicketRepository _taskTicketRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ILLMProviderFactory _llmFactory;
    private readonly IOfficeEventPublisher _eventPublisher;
    private readonly ILogger<TaskWorkflowJob> _logger;

    public TaskWorkflowJob(
        ITaskTicketRepository taskTicketRepository,
        IAgentRepository agentRepository,
        ILLMProviderFactory llmFactory,
        IOfficeEventPublisher eventPublisher,
        ILogger<TaskWorkflowJob> logger)
    {
        _taskTicketRepository = taskTicketRepository;
        _agentRepository = agentRepository;
        _llmFactory = llmFactory;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task ProcessTaskStepAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _taskTicketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            _logger.LogWarning("Không tìm thấy TaskTicket với Id: {TicketId}", ticketId);
            return;
        }

        switch (ticket.CurrentPhase)
        {
            case TaskPhase.Debating:
                await ExecuteDebateAndPlanningAsync(ticket, cancellationToken);
                break;

            case TaskPhase.Executing:
                await ExecuteImplementationAndReviewAsync(ticket, cancellationToken);
                break;

            default:
                _logger.LogInformation("TaskTicket {TicketId} is in phase {Phase}, awaiting user approval or intervention.", ticketId, ticket.CurrentPhase);
                break;
        }
    }

    private async Task ExecuteDebateAndPlanningAsync(TaskTicket ticket, CancellationToken cancellationToken)
    {
        var analysisAgents = await _agentRepository.GetByDepartmentAsync("Strategy & Analysis", cancellationToken);
        var planningAgents = await _agentRepository.GetByDepartmentAsync("Operations & Planning", cancellationToken);

        var analysisSummary = new List<string>();
        foreach (var agent in analysisAgents)
        {
            var provider = _llmFactory.GetProvider(agent.ModelConfig.Provider);
            var prompt = $"Yêu cầu từ người dùng:\n\"{ticket.Description}\"\n\nHãy phân tích yêu cầu dưới góc độ chuyên môn của bạn ({agent.RoleTitle}) và nêu rõ mục tiêu cũng như các điểm lưu ý.";
            var response = await provider.GenerateResponseAsync(agent.ModelConfig, agent.SystemPrompt, prompt, cancellationToken);

            var turn = new DebateTurn(ticket.Id, agent.Id, agent.Name, agent.Avatar, roundNumber: 1, response);
            ticket.AddDebateTurn(turn);
            analysisSummary.Add($"{agent.Name} ({agent.RoleTitle}): {response}");

            await _eventPublisher.PublishAsync(
                OfficeEvent.CreateSpeaking(agent.Name, agent.Avatar, "Phòng Phân tích", TaskPhase.Debating, response),
                cancellationToken);
        }

        // --- BƯỚC 2: PHÒNG KẾ HOẠCH (OPERATIONS & PLANNING) LẬP KẾ HOẠCH & ĐÁNH GIÁ NGUỒN LỰC ---
        var combinedAnalysis = string.Join("\n\n", analysisSummary);
        HiringProposal? proposedHiring = null;

        foreach (var planner in planningAgents)
        {
            var provider = _llmFactory.GetProvider(planner.ModelConfig.Provider);
            var prompt = $$"""
                Bản phân tích từ Phòng Phân tích:
                {{combinedAnalysis}}

                Dưới góc độ chuyên môn ({{planner.RoleTitle}}), hãy đưa ra phương án thực hiện và trả lời dưới định dạng JSON sau:
                {
                  "ProposedPrimaryExecutor": "Tên Agent thực hiện chính hoặc cần tuyển mới",
                  "ProposedTester": "Tên Agent kiểm thử/thẩm định",
                  "NeedsHiring": true/false,
                  "MissingRole": "Tên chức danh nếu cần tuyển (hoặc null)",
                  "MissingSkill": "Kỹ năng cần nếu cần tuyển (hoặc null)",
                  "Reasoning": "Lý do và kế hoạch ngắn gọn"
                }
                """;

            var response = await provider.GenerateResponseAsync(planner.ModelConfig, planner.SystemPrompt, prompt, cancellationToken);
            ticket.AddDebateTurn(new DebateTurn(ticket.Id, planner.Id, planner.Name, planner.Avatar, roundNumber: 2, response));

            await _eventPublisher.PublishAsync(
                OfficeEvent.CreateSpeaking(planner.Name, planner.Avatar, "Phòng Kế hoạch", TaskPhase.Debating, response),
                cancellationToken);

            try
            {
                var cleanJson = ExtractJson(response);
                using var doc = JsonDocument.Parse(cleanJson);
                var root = doc.RootElement;
                var needsHiring = root.GetProperty("NeedsHiring").GetBoolean();

                if (needsHiring && proposedHiring is null)
                {
                    var missingRole = root.GetProperty("MissingRole").GetString() ?? "Specialist";
                    var missingSkill = root.GetProperty("MissingSkill").GetString() ?? "Specialized Skill";
                    var reasoning = root.GetProperty("Reasoning").GetString() ?? "Cần chuyên môn sâu.";

                    proposedHiring = new HiringProposal(missingRole, "Project Operations", missingSkill, reasoning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không parse được JSON vote từ {PlannerName}: {Error}", planner.Name, ex.Message);
            }
        }

        // --- BƯỚC 3: PHÒNG TUYỂN DỤNG & THẨM ĐỊNH AI (TALENT & MODEL INTELLIGENCE) ---
        // Nếu phát hiện lỗ hổng kỹ năng, phòng Tuyển dụng AI sẽ khảo sát benchmark, thiết kế system prompt và tối ưu chi phí
        if (proposedHiring is not null)
        {
            proposedHiring = await ExecuteTalentIntelligenceEvaluationAsync(ticket, proposedHiring, combinedAnalysis, cancellationToken);
        }

        // --- BƯỚC 4: TỔNG HỢP KẾ HOẠCH ĐỒNG THUẬN (CONSENSUS PLAN) ---
        var reviewAgents = await _agentRepository.GetByDepartmentAsync("Quality Control & Review", cancellationToken);
        var defaultTester = reviewAgents.FirstOrDefault();
        var defaultExecutor = analysisAgents.FirstOrDefault();

        var consensusPlan = new ConsensusPlan(
            PrimaryExecutorId: defaultExecutor?.Id ?? Guid.Empty,
            PrimaryExecutorName: defaultExecutor?.Name ?? "Athena",
            TesterId: defaultTester?.Id ?? Guid.Empty,
            TesterName: defaultTester?.Name ?? "Valeria",
            ExecutionPlanSummary: "Kế hoạch đã được thẩm định đồng thuận bởi các phòng ban chuyên môn.",
            RequiresHiring: proposedHiring is not null,
            ProposedHire: proposedHiring);

        ticket.SetConsensusPlan(consensusPlan);
        await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

        if (consensusPlan.RequiresHiring && proposedHiring is not null)
        {
            await _eventPublisher.PublishAsync(
                OfficeEventExtensions.CreateHiringRequested(
                    proposedHiring.RoleTitle,
                    proposedHiring.Department,
                    proposedHiring.RequiredSkill,
                    proposedHiring.Justification,
                    proposedHiring.RecommendedProvider,
                    proposedHiring.RecommendedModel,
                    proposedHiring.ModelBenchmarkRationale),
                cancellationToken);
        }
        else
        {
            // Không cần tuyển người -> Tự động chạy tiếp khâu thực thi
            await ProcessTaskStepAsync(ticket.Id, cancellationToken);
        }
    }

    private async Task ExecuteImplementationAndReviewAsync(TaskTicket ticket, CancellationToken cancellationToken)
    {
        var executor = ticket.RecruitedAgentId.HasValue
            ? await _agentRepository.GetByIdAsync(ticket.RecruitedAgentId.Value, cancellationToken)
            : await _agentRepository.GetByIdAsync(ticket.FinalPlan?.PrimaryExecutorId ?? Guid.Empty, cancellationToken);

        var reviewAgents = await _agentRepository.GetByDepartmentAsync("Quality Control & Review", cancellationToken);

        if (executor is null)
        {
            _logger.LogError("Không tìm thấy Executor cho TaskTicket {TicketId}", ticket.Id);
            return;
        }

        // --- BƯỚC 1: EXECUTOR THỰC HIỆN SẢN PHẨM ---
        var executorProvider = _llmFactory.GetProvider(executor.ModelConfig.Provider);
        var execPrompt = $"""
            Yêu cầu ban đầu từ người dùng:
            "{ticket.Description}"

            Hãy bắt tay vào triển khai và hoàn thành sản phẩm hoàn chỉnh nhất theo đúng vai trò chuyên môn của bạn ({executor.RoleTitle}).
            """;

        var deliverableContent = await executorProvider.GenerateResponseAsync(executor.ModelConfig, executor.SystemPrompt, execPrompt, cancellationToken);

        var deliverable = new TaskDeliverable(
            ticket.Id,
            title: $"{ticket.Title} - Deliverable",
            contentType: "markdown",
            content: deliverableContent,
            createdByAgentId: executor.Id);

        ticket.AddDeliverable(deliverable);

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(executor.Name, executor.Avatar, "Phòng Thẩm định", TaskPhase.Executing,
                "Tôi đã hoàn thiện xong sản phẩm theo yêu cầu. Kính mời Phòng Thẩm định & Kiểm soát Chất lượng tiến hành rà soát!"),
            cancellationToken);

        ticket.StartTesting();
        var reviewPassed = true;

        foreach (var reviewer in reviewAgents)
        {
            var reviewerProvider = _llmFactory.GetProvider(reviewer.ModelConfig.Provider);
            var reviewPrompt = $"""
                Yêu cầu của người dùng:
                "{ticket.Description}"

                Sản phẩm nộp về từ {executor.Name}:
                {deliverableContent}

                Dưới góc độ chuyên môn ({reviewer.RoleTitle}), hãy rà soát kỹ lưỡng sản phẩm này và nhận xét chi tiết.
                """;

            var reviewFeedback = await reviewerProvider.GenerateResponseAsync(reviewer.ModelConfig, reviewer.SystemPrompt, reviewPrompt, cancellationToken);

            var turn = new ExecutionTurn(
                ticket.Id,
                loopNumber: ticket.CurrentFeedbackLoop,
                speakerAgentId: reviewer.Id,
                speakerName: reviewer.Name,
                speakerRoleTitle: reviewer.RoleTitle,
                content: reviewFeedback,
                isIssueFound: false,
                issueDetails: null);

            ticket.RecordExecutionTurn(turn);

            await _eventPublisher.PublishAsync(
                OfficeEvent.CreateSpeaking(reviewer.Name, reviewer.Avatar, executor.Name, TaskPhase.TestingAndReviewing, reviewFeedback),
                cancellationToken);
        }

        // --- BƯỚC 3: NGHIỆM THU & CHỜ PHÊ DUYỆT TỪ NGƯỜI DÙNG (USER APPROVAL) ---
        if (reviewPassed)
        {
            ticket.MarkWaitingFinalApproval();
            await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

            await _eventPublisher.PublishAsync(
                OfficeEventExtensions.CreateWaitingFinalApproval(ticket.Id, ticket.Title),
                cancellationToken);
        }
    }

    private async Task<HiringProposal> ExecuteTalentIntelligenceEvaluationAsync(
        TaskTicket ticket,
        HiringProposal rawProposal,
        string taskAnalysis,
        CancellationToken cancellationToken)
    {
        var talentAgents = await _agentRepository.GetByDepartmentAsync("Talent & Model Intelligence", cancellationToken);
        if (talentAgents.Count == 0)
        {
            return rawProposal;
        }

        string recommendedProvider = "DeepSeek";
        string recommendedModel = "deepseek-reasoner";
        string rationale = "Vượt trội về khả năng suy luận logic chuyên sâu với chi phí tối ưu.";
        string draftPrompt = $"Bạn là chuyên gia {rawProposal.RoleTitle}. Bạn có trách nhiệm thực hiện chính xác các yêu cầu chuyên môn: {rawProposal.RequiredSkill}.";

        foreach (var agent in talentAgents)
        {
            var provider = _llmFactory.GetProvider(agent.ModelConfig.Provider);
            var prompt = $$"""
                Bản phân tích bài toán từ Phòng Phân tích:
                {{taskAnalysis}}

                Phòng Kế hoạch yêu cầu tuyển dụng nhân sự cho vai trò: {{rawProposal.RoleTitle}}
                Kỹ năng chuyên môn cần có: {{rawProposal.RequiredSkill}}
                Lý do yêu cầu: {{rawProposal.Justification}}

                Dưới góc độ chuyên môn của bạn ({{agent.RoleTitle}}), hãy đánh giá và phản hồi:
                - Nếu bạn là Iris (Benchmark Evaluator): Phân tích model AI nào là tối ưu nhất cho kỹ năng này.
                - Nếu bạn là Clio (Prompt Architect): Soạn thảo system prompt chuyên sâu nhất cho vai trò này.
                - Nếu bạn là Selene (Cost & Efficiency): Cân đối chi phí, tốc độ và đưa ra JSON đề xuất hoàn chỉnh sau:
                {
                  "RecommendedProvider": "Tên Provider (OpenAI/Anthropic/DeepSeek/Gemini/Groq/Ollama)",
                  "RecommendedModel": "Tên Model chính xác",
                  "ModelBenchmarkRationale": "Lý do chọn model này dựa trên năng lực vượt trội",
                  "DraftSystemPrompt": "System Prompt hoàn chỉnh cho nhân sự mới"
                }
                """;

            var response = await provider.GenerateResponseAsync(agent.ModelConfig, agent.SystemPrompt, prompt, cancellationToken);
            ticket.AddDebateTurn(new DebateTurn(ticket.Id, agent.Id, agent.Name, agent.Avatar, roundNumber: 3, response));

            await _eventPublisher.PublishAsync(
                OfficeEvent.CreateSpeaking(agent.Name, agent.Avatar, "Phòng Tuyển dụng AI", TaskPhase.Debating, response),
                cancellationToken);

            try
            {
                var cleanJson = ExtractJson(response);
                if (!string.IsNullOrWhiteSpace(cleanJson) && cleanJson.Contains("RecommendedModel"))
                {
                    using var doc = JsonDocument.Parse(cleanJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("RecommendedProvider", out var pProp) && pProp.GetString() is { } p)
                        recommendedProvider = p;
                    if (root.TryGetProperty("RecommendedModel", out var mProp) && mProp.GetString() is { } m)
                        recommendedModel = m;
                    if (root.TryGetProperty("ModelBenchmarkRationale", out var rProp) && rProp.GetString() is { } r)
                        rationale = r;
                    if (root.TryGetProperty("DraftSystemPrompt", out var sProp) && sProp.GetString() is { } s)
                        draftPrompt = s;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không parse được JSON từ {AgentName}: {Error}", agent.Name, ex.Message);
            }
        }

        return rawProposal with
        {
            RecommendedProvider = recommendedProvider,
            RecommendedModel = recommendedModel,
            ModelBenchmarkRationale = rationale,
            DraftSystemPrompt = draftPrompt
        };
    }

    private static string ExtractJson(string text)
    {
        var startIndex = text.IndexOf('{');
        var endIndex = text.LastIndexOf('}');
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
        return text;
    }
}
