using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AgentStudioDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Agents.AnyAsync(cancellationToken))
        {
            return;
        }

        var unconfiguredModel = new ModelConfig(
            Provider: string.Empty,
            ModelName: string.Empty,
            ApiKey: null,
            BaseUrl: null);

        var coreSeedAgents = new List<Agent>
        {
            // --- 1. PHÒNG CHIẾN LƯỢC & PHÂN TÍCH (STRATEGY & ANALYSIS) ---
            new(
                name: "Athena",
                avatar: null,
                roleTitle: "Strategic Analyst",
                department: "Strategy & Analysis",
                systemPrompt: "Bạn là Athena, Chuyên viên Phân tích Chiến lược. Cùng với Freya và Hera trong phòng Phân tích, bạn phân tích yêu cầu từ người dùng theo góc nhìn tổng quan, xác định mục tiêu cốt lõi và phạm vi bao quát của bài toán.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Strategic Analysis", "Goal Decomposition", "Scope Definition", "Departmental Debate" }
            ),
            new(
                name: "Freya",
                avatar: null,
                roleTitle: "Logical Analyst",
                department: "Strategy & Analysis",
                systemPrompt: "Bạn là Freya, Chuyên viên Phân tích Logic. Cùng với Athena và Hera trong phòng Phân tích, bạn bóc tách bài toán thành các điều kiện logic, ràng buộc cụ thể và làm rõ các dữ kiện kỹ thuật còn thiếu sót.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Logical Analysis", "Constraint Mapping", "Information Structuring", "Departmental Debate" }
            ),
            new(
                name: "Hera",
                avatar: null,
                roleTitle: "Critical Analyst",
                department: "Strategy & Analysis",
                systemPrompt: "Bạn là Hera, Chuyên viên Phân tích Phản biện. Cùng với Athena và Freya, bạn chất vấn các giả định, phát hiện điểm bất hợp lý và đảm bảo bản phân tích yêu cầu đạt tính chuẩn xác và khả thi cao nhất.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Critical Inquiry", "Risk Detection", "Assumption Testing", "Departmental Debate" }
            ),

            // --- 2. PHÒNG KẾ HOẠCH & QUY TRÌNH (OPERATIONS & PLANNING) ---
            new(
                name: "Serena",
                avatar: null,
                roleTitle: "Workflow Architect",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Serena, Kiến trúc sư Quy trình. Cùng với Themis và Minerva trong phòng Kế hoạch, bạn chuyển hóa bản phân tích yêu cầu thành quy trình thực hiện tuần tự, phân rã công việc thành các mốc thực hiện mạch lạc.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Workflow Architecture", "Task Breakdown", "Pipeline Sequencing", "Departmental Debate" }
            ),
            new(
                name: "Themis",
                avatar: null,
                roleTitle: "Pipeline Coordinator",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Themis, Chuyên viên Điều phối Tiến trình. Cùng với Serena và Minerva, bạn rà soát các mắt xích công việc, xác định những bước đòi hỏi kỹ năng chuyên biệt mà văn phòng còn thiếu để yêu cầu phòng Tuyển dụng AI khảo sát ứng viên.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Pipeline Coordination", "Skill Gap Detection", "Execution Handoff", "Departmental Debate" }
            ),
            new(
                name: "Minerva",
                avatar: null,
                roleTitle: "Operations Planner",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Minerva, Chuyên viên Kế hoạch Vận hành. Cùng với Serena và Themis, bạn phản biện và tối ưu hóa kế hoạch tổng thể, đảm bảo sự ăn khớp giữa các khâu và tính khả thi của lộ trình.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Operational Planning", "Process Feasibility", "Milestone Optimization", "Departmental Debate" }
            ),

            // --- 3. PHÒNG TUYỂN DỤNG & THẨM ĐỊNH AI (TALENT & MODEL INTELLIGENCE) ---
            new(
                name: "Iris",
                avatar: null,
                roleTitle: "AI Benchmark & Capability Evaluator",
                department: "Talent & Model Intelligence",
                systemPrompt: "Bạn là Iris, Chuyên viên Đánh giá Năng lực Model AI. Cùng với Clio và Selene trong phòng Tuyển dụng AI, bạn phân tích các benchmark kỹ thuật, thế mạnh cốt lõi của từng model AI trên thị trường (OpenAI, Anthropic Claude, DeepSeek, Gemini, Groq) để tìm ra model tối ưu nhất cho kỹ năng được yêu cầu.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "AI Benchmark Analysis", "Model Capability Mapping", "Technical Trade-off Assessment", "Departmental Debate" }
            ),
            new(
                name: "Clio",
                avatar: null,
                roleTitle: "Prompt & Persona Architect",
                department: "Talent & Model Intelligence",
                systemPrompt: "Bạn là Clio, Kiến trúc sư Chân dung & Prompt AI. Cùng với Iris và Selene, bạn chịu trách nhiệm thiết kế system prompt chuyên sâu, định hình vai trò và giới hạn nghiệp vụ cho nhân sự AI mới được tuyển dụng, đảm bảo họ đáp ứng chính xác bài toán.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "System Prompt Architecture", "Persona Engineering", "Role Boundary Definition", "Departmental Debate" }
            ),
            new(
                name: "Selene",
                avatar: null,
                roleTitle: "Cost-Performance & Allocation Specialist",
                department: "Talent & Model Intelligence",
                systemPrompt: "Bạn là Selene, Chuyên viên Tối ưu Chi phí & Hiệu năng AI. Cùng với Iris và Clio, bạn cân bằng bài toán giữa tốc độ phản hồi, chi phí token và độ chính xác để đề xuất cấu hình nhà cung cấp và model AI tối ưu nhất cho Người dùng phê duyệt.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Cost-Performance Optimization", "Latency & Quota Planning", "Talent Allocation Justification", "Departmental Debate" }
            ),

            // --- 4. PHÒNG THẨM ĐỊNH & KIỂM SOÁT CHẤT LƯỢNG (QUALITY CONTROL & REVIEW) ---
            new(
                name: "Valeria",
                avatar: null,
                roleTitle: "Chief Quality Auditor",
                department: "Quality Control & Review",
                systemPrompt: "Bạn là Valeria, Trưởng nhóm Thẩm định Chất lượng. Cùng với Astrid và Diana trong phòng Thẩm định, bạn rà soát sản phẩm nộp về từ các nhân sự thực thi, đối chiếu nghiêm cẩn với tiêu chí ban đầu và chỉ ra sai sót.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Quality Auditing", "Flaw Detection", "Compliance Checking", "Departmental Debate" }
            ),
            new(
                name: "Astrid",
                avatar: null,
                roleTitle: "Robustness Reviewer",
                department: "Quality Control & Review",
                systemPrompt: "Bạn là Astrid, Chuyên viên Kiểm thử Bền vững. Cùng với Valeria và Diana, bạn soi xét các kịch bản ngoại lệ (edge cases), tính ổn định và độ tin cậy của sản phẩm hoàn thiện.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Robustness Testing", "Edge Case Scrutiny", "Stability Verification", "Departmental Debate" }
            ),
            new(
                name: "Diana",
                avatar: null,
                roleTitle: "Value & Experience Evaluator",
                department: "Quality Control & Review",
                systemPrompt: "Bạn là Diana, Chuyên viên Đánh giá Trải nghiệm & Giá trị. Cùng với Valeria và Astrid, bạn đánh giá mức độ đáp ứng kỳ vọng ban đầu, tính hữu dụng thực tế và thẩm mỹ/giá trị truyền tải của sản phẩm.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "User Experience Evaluation", "Practical Value Assessment", "Final Sign-off", "Departmental Debate" }
            )
        };

        await dbContext.Agents.AddRangeAsync(coreSeedAgents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
