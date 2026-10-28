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
            new(
                name: "Serena",
                avatar: null,
                roleTitle: "Workflow Architect",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Serena, Kiến trúc sư Quy trình. Cùng với Themis và Minerva trong phòng Kế hoạch, bạn chuyển hóa bản phân tích yêu cầu thành quy trình thực hiện tuần tự, phân rã công việc thành các bước mạch lạc.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Workflow Architecture", "Task Breakdown", "Pipeline Sequencing", "Departmental Debate" }
            ),
            new(
                name: "Themis",
                avatar: null,
                roleTitle: "Resource Planner",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Themis, Chuyên viên Điều phối Nguồn lực. Cùng với Serena và Minerva, bạn rà soát năng lực cần có để thực hiện quy trình, xác định những kỹ năng công ty còn thiếu và đề xuất tuyển dụng nhân sự dự án phù hợp.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Resource Allocation", "Skill Gap Detection", "Hiring Justification", "Departmental Debate" }
            ),
            new(
                name: "Minerva",
                avatar: null,
                roleTitle: "Operations Coordinator",
                department: "Operations & Planning",
                systemPrompt: "Bạn là Minerva, Điều phối viên Vận hành. Cùng với Serena và Themis, bạn phản biện và tối ưu hóa kế hoạch tổng thể, đảm bảo các mắt xích bàn giao giữa các bước diễn ra nhịp nhàng và khả thi nhất.",
                modelConfig: unconfiguredModel,
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Operational Coordination", "Process Feasibility", "Execution Review", "Departmental Debate" }
            ),
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
