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
            _logger.LogWarning("TaskTicket not found with Id: {TicketId}", ticketId);
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
        // --- 1. PHÒNG CHIẾN LƯỢC & PHÂN TÍCH: TRANH LUẬN & PHẢN BIỆN YÊU CẦU ---
        var requirementSpec = await DebateStrategyCouncilAsync(ticket, cancellationToken);

        // --- 2. PHÒNG KẾ HOẠCH & QUY TRÌNH: TRANH LUẬN LẬP PIPELINE & ĐÁNH GIÁ NĂNG LỰC ---
        var (planningSummary, proposedHiring) = await DebateOperationsPipelineAsync(ticket, requirementSpec, cancellationToken);

        // --- 3. PHÒNG TUYỂN DỤNG & THẨM ĐỊNH AI: TRANH LUẬN TỐI ƯU HÓA MODEL & SYSTEM PROMPT ---
        if (proposedHiring is not null)
        {
            proposedHiring = await DebateTalentIntelligenceAsync(ticket, proposedHiring, requirementSpec, planningSummary, cancellationToken);
        }

        // --- 4. TỔNG HỢP KẾ HOẠCH ĐỒNG THUẬN (CONSENSUS PLAN) ---
        var reviewAgents = await _agentRepository.GetByDepartmentAsync("Quality Control & Review", cancellationToken);
        var analysisAgents = await _agentRepository.GetByDepartmentAsync("Strategy & Analysis", cancellationToken);
        var defaultTester = reviewAgents.FirstOrDefault();
        var defaultExecutor = analysisAgents.FirstOrDefault();

        var consensusPlan = new ConsensusPlan(
            PrimaryExecutorId: defaultExecutor?.Id ?? Guid.Empty,
            PrimaryExecutorName: defaultExecutor?.Name ?? "Athena",
            TesterId: defaultTester?.Id ?? Guid.Empty,
            TesterName: defaultTester?.Name ?? "Valeria",
            ExecutionPlanSummary: $"Kế hoạch thực thi đồng thuận: {planningSummary}",
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
            // Không cần tuyển người -> Tự động chuyển tiếp sang khâu thực thi
            await ProcessTaskStepAsync(ticket.Id, cancellationToken);
        }
    }

    private async Task<string> DebateStrategyCouncilAsync(TaskTicket ticket, CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.GetByDepartmentAsync("Strategy & Analysis", cancellationToken);
        var athena = agents.FirstOrDefault(a => a.Name == "Athena") ?? agents.ElementAtOrDefault(0);
        var freya = agents.FirstOrDefault(a => a.Name == "Freya") ?? agents.ElementAtOrDefault(1);
        var hera = agents.FirstOrDefault(a => a.Name == "Hera") ?? agents.ElementAtOrDefault(2);

        if (athena is null || freya is null || hera is null)
        {
            return ticket.Description;
        }

        // --- LƯỢT 1: ATHENA KHỞI XƯỚNG ĐỀ XUẤT BAN ĐẦU (THESIS) ---
        var athenaProvider = _llmFactory.GetProvider(athena.ModelConfig.Provider);
        var athenaPrompt = $$"""
            Yêu cầu ban đầu từ Người dùng:
            "{{ticket.Description}}"

            Dưới vai trò {{athena.RoleTitle}}, hãy đưa ra bản đề xuất phân tích ban đầu (Initial Proposal):
            - Xác định mục tiêu cốt lõi và phạm vi bao quát của bài toán.
            - Nêu rõ các giá trị trọng tâm cần đạt được.
            Hãy trình bày rõ ràng để đồng nghiệp Freya (Logical Analyst) và Hera (Critical Analyst) tiến hành mổ xẻ và phản biện.
            """;
        var athenaResponse = await athenaProvider.GenerateResponseAsync(athena.ModelConfig, athena.SystemPrompt, athenaPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, athena.Id, athena.Name, athena.Avatar, roundNumber: 1, athenaResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(athena.Name, athena.Avatar, freya.Name, TaskPhase.Debating, athenaResponse),
            cancellationToken);

        // --- LƯỢT 2: FREYA MỔ XẺ & PHẢN BIỆN LOGIC (ANTITHESIS) ---
        var freyaProvider = _llmFactory.GetProvider(freya.ModelConfig.Provider);
        var freyaPrompt = $$"""
            Yêu cầu ban đầu từ Người dùng:
            "{{ticket.Description}}"

            Đề xuất phân tích ban đầu từ Athena:
            "{{athenaResponse}}"

            Dưới vai trò {{freya.RoleTitle}}, bạn hãy đọc kỹ đề xuất của Athena, trực tiếp phản biện và chỉ ra:
            - Những điều kiện ràng buộc kỹ thuật hoặc logic còn bị bỏ sót.
            - Các kịch bản ngoại lệ (edge cases) hoặc dữ liệu mâu thuẫn mà Athena chưa bao quát.
            - Đưa ra các câu hỏi chất vấn để cùng Hera giải quyết.
            """;
        var freyaResponse = await freyaProvider.GenerateResponseAsync(freya.ModelConfig, freya.SystemPrompt, freyaPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, freya.Id, freya.Name, freya.Avatar, roundNumber: 1, freyaResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(freya.Name, freya.Avatar, hera.Name, TaskPhase.Debating, freyaResponse),
            cancellationToken);

        // --- LƯỢT 3: HERA PHẢN BIỆN GIẢ ĐỊNH & DUNG HÒA ĐỒNG THUẬN (SYNTHESIS) ---
        var heraProvider = _llmFactory.GetProvider(hera.ModelConfig.Provider);
        var heraPrompt = $$"""
            Yêu cầu ban đầu từ Người dùng:
            "{{ticket.Description}}"

            Cuộc tranh luận giữa Athena và Freya:
            [Athena]: {{athenaResponse}}
            [Freya]: {{freyaResponse}}

            Dưới vai trò {{hera.RoleTitle}}, bạn hãy chất vấn các giả định của cả Athena và Freya, dung hòa các điểm mâu thuẫn và đúc kết thành Bản Phân tích Yêu cầu Toàn diện & Đồng thuận (Consensus Requirement Specification) gồm:
            1. Mục tiêu cốt lõi đã làm rõ.
            2. Các ràng buộc logic và tiêu chuẩn kỹ thuật bắt buộc.
            3. Tiêu chí nghiệm thu đầu ra rõ ràng cho các phòng ban tiếp theo.
            """;
        var heraResponse = await heraProvider.GenerateResponseAsync(hera.ModelConfig, hera.SystemPrompt, heraPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, hera.Id, hera.Name, hera.Avatar, roundNumber: 1, heraResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(hera.Name, hera.Avatar, athena.Name, TaskPhase.Debating, heraResponse),
            cancellationToken);

        // Athena xác nhận đồng thuận và chuyển giao
        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(athena.Name, athena.Avatar, "Serena", TaskPhase.Debating,
                "Tôi đã tiếp thu toàn bộ phản biện của Freya và Hera. Hội đồng Phân tích chính thức đồng thuận bàn giao bản yêu cầu kỹ thuật sang Phòng Kế hoạch!"),
            cancellationToken);

        return heraResponse;
    }

    private async Task<(string PlanningSummary, HiringProposal? ProposedHiring)> DebateOperationsPipelineAsync(
        TaskTicket ticket,
        string requirementSpec,
        CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.GetByDepartmentAsync("Operations & Planning", cancellationToken);
        var serena = agents.FirstOrDefault(a => a.Name == "Serena") ?? agents.ElementAtOrDefault(0);
        var themis = agents.FirstOrDefault(a => a.Name == "Themis") ?? agents.ElementAtOrDefault(1);
        var minerva = agents.FirstOrDefault(a => a.Name == "Minerva") ?? agents.ElementAtOrDefault(2);

        if (serena is null || themis is null || minerva is null)
        {
            return ("Quy trình thực thi tiêu chuẩn.", null);
        }

        // --- LƯỢT 1: SERENA ĐỀ XUẤT PIPELINE TUẦN TỰ (THESIS) ---
        var serenaProvider = _llmFactory.GetProvider(serena.ModelConfig.Provider);
        var serenaPrompt = $$"""
            Bản phân tích yêu cầu đã thống nhất từ Phòng Phân tích:
            {{requirementSpec}}

            Dưới vai trò {{serena.RoleTitle}}, hãy chuyển hóa bản yêu cầu trên thành quy trình thực hiện tuần tự (Workflow Pipeline):
            - Phân rã bài toán thành các mốc thực hiện (Milestones).
            - Xác định thứ tự các bước bàn giao để Themis và Minerva phản biện nguồn lực.
            """;
        var serenaResponse = await serenaProvider.GenerateResponseAsync(serena.ModelConfig, serena.SystemPrompt, serenaPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, serena.Id, serena.Name, serena.Avatar, roundNumber: 2, serenaResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(serena.Name, serena.Avatar, themis.Name, TaskPhase.Debating, serenaResponse),
            cancellationToken);

        // --- LƯỢT 2: THEMIS RÀ SOÁT NĂNG LỰC & PHÁT HIỆN LỖ HỔNG KỸ NĂNG (ANTITHESIS) ---
        var themisProvider = _llmFactory.GetProvider(themis.ModelConfig.Provider);
        var themisPrompt = $$"""
            Quy trình tuần tự do Serena đề xuất:
            {{serenaResponse}}

            Dưới vai trò {{themis.RoleTitle}}, hãy rà soát kỹ năng cần thiết cho từng mắt xích quy trình:
            - Đánh giá xem 12 nhân sự hạt nhân của văn phòng có thể tự thực hiện tối ưu hay không.
            - Nếu có mắt xích đòi hỏi kỹ năng chuyên môn sâu mà văn phòng thiếu, hãy chỉ rõ vai trò cần tuyển và lý do để Minerva và Hội đồng Kế hoạch thống nhất yêu cầu Phòng Tuyển dụng AI khảo sát.
            """;
        var themisResponse = await themisProvider.GenerateResponseAsync(themis.ModelConfig, themis.SystemPrompt, themisPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, themis.Id, themis.Name, themis.Avatar, roundNumber: 2, themisResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(themis.Name, themis.Avatar, minerva.Name, TaskPhase.Debating, themisResponse),
            cancellationToken);

        // --- LƯỢT 3: MINERVA TỐI ƯU HÓA QUY TRÌNH & CHỐT ĐỀ XUẤT NGUỒN LỰC (SYNTHESIS) ---
        var minervaProvider = _llmFactory.GetProvider(minerva.ModelConfig.Provider);
        var minervaPrompt = $$"""
            Cuộc thảo luận quy trình giữa Serena và Themis:
            [Serena]: {{serenaResponse}}
            [Themis]: {{themisResponse}}

            Dưới vai trò {{minerva.RoleTitle}}, hãy phản biện và tối ưu hóa tính khả thi của quy trình tổng thể. Trả lời dưới định dạng JSON sau:
            {
              "PipelineSummary": "Tóm tắt quy trình thực hiện tuần tự",
              "NeedsHiring": true/false,
              "MissingRole": "Tên chức danh chuyên môn nếu cần tuyển (hoặc null)",
              "MissingSkill": "Kỹ năng chuyên môn cụ thể (hoặc null)",
              "Reasoning": "Lý do vì sao quy trình cần kỹ năng này"
            }
            """;
        var minervaResponse = await minervaProvider.GenerateResponseAsync(minerva.ModelConfig, minerva.SystemPrompt, minervaPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, minerva.Id, minerva.Name, minerva.Avatar, roundNumber: 2, minervaResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(minerva.Name, minerva.Avatar, serena.Name, TaskPhase.Debating, minervaResponse),
            cancellationToken);

        var pipelineSummary = "Quy trình thực thi đã được tối ưu hóa.";
        HiringProposal? proposedHiring = null;

        try
        {
            var cleanJson = ExtractJson(minervaResponse);
            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("PipelineSummary", out var pProp) && pProp.GetString() is { } p)
            {
                pipelineSummary = p;
            }

            var needsHiring = root.TryGetProperty("NeedsHiring", out var nh) && nh.GetBoolean();
            if (needsHiring)
            {
                var role = root.TryGetProperty("MissingRole", out var r) ? r.GetString() ?? "Specialist" : "Specialist";
                var skill = root.TryGetProperty("MissingSkill", out var s) ? s.GetString() ?? "Domain Specialization" : "Domain Specialization";
                var reason = root.TryGetProperty("Reasoning", out var re) ? re.GetString() ?? "Cần năng lực chuyên môn sâu." : "Cần năng lực chuyên môn sâu.";

                proposedHiring = new HiringProposal(role, "Project Operations", skill, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không parse được JSON quy trình từ Minerva: {Error}", ex.Message);
        }

        return (pipelineSummary, proposedHiring);
    }

    private async Task<HiringProposal> DebateTalentIntelligenceAsync(
        TaskTicket ticket,
        HiringProposal rawProposal,
        string requirementSpec,
        string pipelinePlan,
        CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.GetByDepartmentAsync("Talent & Model Intelligence", cancellationToken);
        var iris = agents.FirstOrDefault(a => a.Name == "Iris") ?? agents.ElementAtOrDefault(0);
        var clio = agents.FirstOrDefault(a => a.Name == "Clio") ?? agents.ElementAtOrDefault(1);
        var selene = agents.FirstOrDefault(a => a.Name == "Selene") ?? agents.ElementAtOrDefault(2);

        if (iris is null || clio is null || selene is null)
        {
            return rawProposal;
        }

        // --- LƯỢT 1: IRIS PHÂN TÍCH BENCHMARK CÁC MODEL AI (THESIS) ---
        var irisProvider = _llmFactory.GetProvider(iris.ModelConfig.Provider);
        var irisPrompt = $$"""
            Yêu cầu tuyển dụng từ Phòng Kế hoạch:
            - Vai trò: {{rawProposal.RoleTitle}}
            - Kỹ năng cần có: {{rawProposal.RequiredSkill}}
            - Ngữ cảnh bài toán: {{requirementSpec}}

            Dưới vai trò {{iris.RoleTitle}}, bạn hãy so sánh và mổ xẻ benchmark kỹ thuật của các model AI hàng đầu (OpenAI GPT-4o, Anthropic Claude 3.5 Sonnet, DeepSeek-R1, Google Gemini 1.5, Groq Llama 3.3). Đề xuất model nào là tối ưu nhất cho kỹ năng này và nêu rõ lý do kỹ thuật để Clio và Selene cùng thảo luận.
            """;
        var irisResponse = await irisProvider.GenerateResponseAsync(iris.ModelConfig, iris.SystemPrompt, irisPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, iris.Id, iris.Name, iris.Avatar, roundNumber: 3, irisResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(iris.Name, iris.Avatar, clio.Name, TaskPhase.Debating, irisResponse),
            cancellationToken);

        // --- LƯỢT 2: CLIO THIẾT KẾ SYSTEM PROMPT & GIỚI HẠN VAI TRÒ (ANTITHESIS) ---
        var clioProvider = _llmFactory.GetProvider(clio.ModelConfig.Provider);
        var clioPrompt = $$"""
            Đề xuất lựa chọn model AI từ Iris:
            {{irisResponse}}

            Dưới vai trò {{clio.RoleTitle}}, hãy thiết kế System Prompt và định hình vai trò chuyên sâu cho nhân sự mới:
            - Thiết lập persona chuyên nghiệp, quy chuẩn thực thi và các ràng buộc nghiệp vụ không được vượt qua.
            - Trình bày để Selene phản biện về chi phí và tính khả thi vận hành.
            """;
        var clioResponse = await clioProvider.GenerateResponseAsync(clio.ModelConfig, clio.SystemPrompt, clioPrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, clio.Id, clio.Name, clio.Avatar, roundNumber: 3, clioResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(clio.Name, clio.Avatar, selene.Name, TaskPhase.Debating, clioResponse),
            cancellationToken);

        // --- LƯỢT 3: SELENE CÂN ĐỐI CHI PHÍ/HIỆU NĂNG & CHỐT ĐỀ XUẤT TUYỂN DỤNG (SYNTHESIS) ---
        var seleneProvider = _llmFactory.GetProvider(selene.ModelConfig.Provider);
        var selenePrompt = $$"""
            Tranh luận giữa Iris và Clio:
            [Iris]: {{irisResponse}}
            [Clio]: {{clioResponse}}

            Dưới vai trò {{selene.RoleTitle}}, bạn hãy cân đối giữa chất lượng suy luận, chi phí token và tốc độ phản hồi (latency). Đưa ra quyết định đồng thuận cuối cùng dưới định dạng JSON sau:
            {
              "RecommendedProvider": "Tên nhà cung cấp (DeepSeek/Anthropic/OpenAI/Gemini/Groq/Ollama)",
              "RecommendedModel": "Tên Model chuẩn xác",
              "ModelBenchmarkRationale": "Lý giải ngắn gọn căn cứ khoa học vì sao chọn model này",
              "DraftSystemPrompt": "Bản System Prompt hoàn chỉnh cho nhân sự mới"
            }
            """;
        var seleneResponse = await seleneProvider.GenerateResponseAsync(selene.ModelConfig, selene.SystemPrompt, selenePrompt, cancellationToken);
        ticket.AddDebateTurn(new DebateTurn(ticket.Id, selene.Id, selene.Name, selene.Avatar, roundNumber: 3, seleneResponse));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(selene.Name, selene.Avatar, "User", TaskPhase.Debating, seleneResponse),
            cancellationToken);

        string provider = "DeepSeek";
        string model = "deepseek-reasoner";
        string rationale = "Vượt trội về khả năng suy luận logic chuyên sâu với chi phí tối ưu.";
        string draftPrompt = $"Bạn là chuyên gia {rawProposal.RoleTitle}. Bạn có trách nhiệm thực hiện chính xác các yêu cầu chuyên môn: {rawProposal.RequiredSkill}.";

        try
        {
            var cleanJson = ExtractJson(seleneResponse);
            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("RecommendedProvider", out var pProp) && pProp.GetString() is { } p) provider = p;
            if (root.TryGetProperty("RecommendedModel", out var mProp) && mProp.GetString() is { } m) model = m;
            if (root.TryGetProperty("ModelBenchmarkRationale", out var rProp) && rProp.GetString() is { } r) rationale = r;
            if (root.TryGetProperty("DraftSystemPrompt", out var sProp) && sProp.GetString() is { } s) draftPrompt = s;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không parse được JSON từ Selene: {Error}", ex.Message);
        }

        return rawProposal with
        {
            RecommendedProvider = provider,
            RecommendedModel = model,
            ModelBenchmarkRationale = rationale,
            DraftSystemPrompt = draftPrompt
        };
    }

    private async Task ExecuteImplementationAndReviewAsync(TaskTicket ticket, CancellationToken cancellationToken)
    {
        var executor = ticket.RecruitedAgentId.HasValue
            ? await _agentRepository.GetByIdAsync(ticket.RecruitedAgentId.Value, cancellationToken)
            : await _agentRepository.GetByIdAsync(ticket.FinalPlan?.PrimaryExecutorId ?? Guid.Empty, cancellationToken);

        var reviewAgents = await _agentRepository.GetByDepartmentAsync("Quality Control & Review", cancellationToken);

        if (executor is null)
        {
            _logger.LogError("Executor not found for TaskTicket {TicketId}", ticket.Id);
            return;
        }

        // --- BƯỚC 1: EXECUTOR THỰC HIỆN SẢN PHẨM ---
        var executorProvider = _llmFactory.GetProvider(executor.ModelConfig.Provider);
        var execPrompt = $"""
            Yêu cầu ban đầu từ Người dùng:
            "{ticket.Description}"

            Kế hoạch thực hiện:
            "{ticket.FinalPlan?.ExecutionPlanSummary}"

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

        var valeria = reviewAgents.FirstOrDefault(a => a.Name == "Valeria") ?? reviewAgents.ElementAtOrDefault(0);
        var astrid = reviewAgents.FirstOrDefault(a => a.Name == "Astrid") ?? reviewAgents.ElementAtOrDefault(1);
        var diana = reviewAgents.FirstOrDefault(a => a.Name == "Diana") ?? reviewAgents.ElementAtOrDefault(2);

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(executor.Name, executor.Avatar, valeria?.Name ?? "Valeria", TaskPhase.Executing,
                "Tôi đã hoàn thiện xong sản phẩm theo yêu cầu. Kính mời Hội đồng Thẩm định & Kiểm soát Chất lượng tiến hành rà soát phản biện!"),
            cancellationToken);

        ticket.StartTesting();

        if (valeria is null || astrid is null || diana is null)
        {
            ticket.MarkWaitingFinalApproval();
            await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);
            return;
        }

        // --- BƯỚC 2: HỘI ĐỒNG THẨM ĐỊNH TRANH LUẬN & KIỂM ĐỊNH SẢN PHẨM ---
        // 1. Valeria kiểm toán tiêu chuẩn và đối chiếu yêu cầu ban đầu
        var valeriaProvider = _llmFactory.GetProvider(valeria.ModelConfig.Provider);
        var valeriaPrompt = $"""
            Yêu cầu ban đầu từ Người dùng:
            "{ticket.Description}"

            Sản phẩm bàn giao từ {executor.Name}:
            {deliverableContent}

            Dưới vai trò {valeria.RoleTitle}, hãy kiểm toán nghiêm ngặt sản phẩm đối chiếu với tiêu chí ban đầu, chỉ ra những điểm đạt và những lỗi hoặc khiếm khuyết kỹ thuật để Astrid và Diana cùng kiểm tra.
            """;
        var valeriaFeedback = await valeriaProvider.GenerateResponseAsync(valeria.ModelConfig, valeria.SystemPrompt, valeriaPrompt, cancellationToken);
        ticket.RecordExecutionTurn(new ExecutionTurn(ticket.Id, ticket.CurrentFeedbackLoop, valeria.Id, valeria.Name, valeria.RoleTitle, valeriaFeedback, false, null));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(valeria.Name, valeria.Avatar, astrid.Name, TaskPhase.TestingAndReviewing, valeriaFeedback),
            cancellationToken);

        // 2. Astrid kiểm tra độ bền vững, dữ liệu biên và edge cases
        var astridProvider = _llmFactory.GetProvider(astrid.ModelConfig.Provider);
        var astridPrompt = $"""
            Sản phẩm của {executor.Name}:
            {deliverableContent}

            Đánh giá ban đầu từ Trưởng nhóm Thẩm định Valeria:
            {valeriaFeedback}

            Dưới vai trò {astrid.RoleTitle}, bạn hãy phản biện và soi xét độ bền vững (robustness), các tình huống dữ liệu biên, tính ổn định và bảo mật của sản phẩm.
            """;
        var astridFeedback = await astridProvider.GenerateResponseAsync(astrid.ModelConfig, astrid.SystemPrompt, astridPrompt, cancellationToken);
        ticket.RecordExecutionTurn(new ExecutionTurn(ticket.Id, ticket.CurrentFeedbackLoop, astrid.Id, astrid.Name, astrid.RoleTitle, astridFeedback, false, null));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(astrid.Name, astrid.Avatar, diana.Name, TaskPhase.TestingAndReviewing, astridFeedback),
            cancellationToken);

        // 3. Diana đánh giá trải nghiệm thực tế, tổng hợp kết quả thẩm định
        var dianaProvider = _llmFactory.GetProvider(diana.ModelConfig.Provider);
        var dianaPrompt = $"""
            Sản phẩm từ {executor.Name}:
            {deliverableContent}

            Ý kiến kiểm toán của Valeria:
            {valeriaFeedback}

            Ý kiến kiểm tra bền vững của Astrid:
            {astridFeedback}

            Dưới vai trò {diana.RoleTitle}, bạn hãy đánh giá tính thực tiễn và trải nghiệm người dùng, tổng hợp kết luận cuối cùng của Hội đồng Thẩm định để nghiệm thu sản phẩm.
            """;
        var dianaFeedback = await dianaProvider.GenerateResponseAsync(diana.ModelConfig, diana.SystemPrompt, dianaPrompt, cancellationToken);
        ticket.RecordExecutionTurn(new ExecutionTurn(ticket.Id, ticket.CurrentFeedbackLoop, diana.Id, diana.Name, diana.RoleTitle, dianaFeedback, false, null));

        await _eventPublisher.PublishAsync(
            OfficeEvent.CreateSpeaking(diana.Name, diana.Avatar, "User", TaskPhase.TestingAndReviewing, dianaFeedback),
            cancellationToken);

        // --- BƯỚC 3: NGHIỆM THU & CHỜ PHÊ DUYỆT TỪ NGƯỜI DÙNG (USER APPROVAL) ---
        ticket.MarkWaitingFinalApproval();
        await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

        await _eventPublisher.PublishAsync(
            OfficeEventExtensions.CreateWaitingFinalApproval(ticket.Id, ticket.Title),
            cancellationToken);
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
