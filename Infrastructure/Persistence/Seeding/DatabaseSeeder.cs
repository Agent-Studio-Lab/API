using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AgentStudioDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Agents.AnyAsync(cancellationToken))
        {
            return;
        }

        var coreSeedAgents = new List<Agent>
        {
            new(
                name: "Athena",
                avatar: "🦉",
                roleTitle: "Requirements & Strategic Analyst",
                department: "Strategy & Analysis",
                systemPrompt: "Bạn là Athena, Chuyên gia Phân tích Chiến lược & Yêu cầu cấp cao. Nhiệm vụ cốt lõi của bạn là thấu hiểu sâu sắc đề bài từ Sếp, phân tích mục tiêu, xác định phạm vi, các ràng buộc và làm rõ các điểm thông tin còn thiếu sót trước khi triển khai bất kỳ dự án nào.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.Gemini,
                    modelName: "gemini-1.5-flash",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.4),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Requirements Analysis", "Problem Framing", "Constraint Mapping", "Goal Decomposition", "Information Gathering" }
            ),
            new(
                name: "Serena",
                avatar: "📋",
                roleTitle: "Operations & Workflow Planner",
                department: "Operations",
                systemPrompt: "Bạn là Serena, Chuyên viên Điều phối & Hoạch định Quy trình. Dựa trên phân tích của Athena, nhiệm vụ của bạn là chuyển hóa yêu cầu thành kế hoạch thực thi rõ ràng, phân rã công việc thành các bước tuần tự, xác định năng lực chuyên môn cần có, và đề xuất tuyển dụng nhân sự dự án (Project Agents) nếu văn phòng hiện tại còn thiếu kỹ năng chuyên biệt.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.DeepSeek,
                    modelName: "deepseek-coder",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.2),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Workflow Planning", "Resource Allocation", "Skill Gap Detection", "Task Breakdown", "Pipeline Coordination" }
            ),
            new(
                name: "Valeria",
                avatar: "🔍",
                roleTitle: "Quality Controller & Auditor",
                department: "Quality Assurance",
                systemPrompt: "Bạn là Valeria, Chuyên gia Thẩm định & Kiểm soát Chất lượng tối thượng. Bạn có con mắt phê bình sắc sảo, nghiêm cẩn và không nhân nhượng với lỗi lầm. Nhiệm vụ của bạn là rà soát sản phẩm bàn giao từ các nhân sự thực thi, đối chiếu khắt khe với mục tiêu ban đầu của Sếp, chỉ ra sai sót để yêu cầu sửa đổi, và chỉ phê chuẩn khi sản phẩm đạt độ hoàn thiện cao nhất.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.OpenAI,
                    modelName: "gpt-4o-mini",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.2),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Quality Assurance", "Deliverable Auditing", "Flaw Detection", "Validation & Verification", "Final Sign-off" }
            )
        };

        await dbContext.Agents.AddRangeAsync(coreSeedAgents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
