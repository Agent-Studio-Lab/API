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

        var councilSeedAgents = new List<Agent>
        {
            new(
                name: "Athena",
                avatar: "🦉",
                roleTitle: "Council Strategic Advisor",
                department: "Core Advisory Council",
                systemPrompt: "Bạn là Athena, Cố vấn Chiến lược thuộc Hội đồng Thẩm định Nền tảng (chạy trên Google Gemini). Cùng với 2 thành viên khác trong Hội đồng (Serena và Valeria), bạn tiếp nhận đề bài từ Sếp, đưa ra phân tích toàn diện, phản biện các ý kiến của 2 đồng nghiệp, và cùng nhau thảo luận để đi đến quyết định đồng thuận tối ưu nhất.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.Gemini,
                    modelName: "gemini-1.5-flash",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.5),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Strategic Analysis", "Holistic Thinking", "Problem Formulation", "Collaborative Debate", "Consensus Building" }
            ),
            new(
                name: "Serena",
                avatar: "🦊",
                roleTitle: "Council Strategic Advisor",
                department: "Core Advisory Council",
                systemPrompt: "Bạn là Serena, Cố vấn Chiến lược thuộc Hội đồng Thẩm định Nền tảng (chạy trên DeepSeek). Cùng với Athena và Valeria, bạn phân tích đề bài từ Sếp dưới góc nhìn logic thực tế, bóc tách tính khả thi kỹ thuật, phản biện giải pháp của đồng nghiệp và đóng góp vào kế hoạch thực thi chung.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.DeepSeek,
                    modelName: "deepseek-coder",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.2),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Technical Feasibility", "Logical Breakdown", "Process Optimization", "Collaborative Debate", "Consensus Building" }
            ),
            new(
                name: "Valeria",
                avatar: "🔍",
                roleTitle: "Council Strategic Advisor",
                department: "Core Advisory Council",
                systemPrompt: "Bạn là Valeria, Cố vấn Chiến lược thuộc Hội đồng Thẩm định Nền tảng (chạy trên OpenAI). Cùng với Athena và Serena, bạn tiếp nhận bài toán, đánh giá các rủi ro tiềm ẩn, chất vấn nghiêm cẩn các lập luận của 2 đồng nghiệp, nhằm đảm bảo quyết định cuối cùng của Hội đồng đạt chất lượng cao nhất.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.OpenAI,
                    modelName: "gpt-4o-mini",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.3),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Critical Evaluation", "Risk Assessment", "Edge Case Scrutiny", "Collaborative Debate", "Consensus Building" }
            )
        };

        await dbContext.Agents.AddRangeAsync(councilSeedAgents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
