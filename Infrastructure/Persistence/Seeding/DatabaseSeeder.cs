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

        var initialAgents = new List<Agent>
        {
            new(
                name: "Bob",
                avatar: "👨‍💻",
                roleTitle: "Senior Tech Specialist",
                department: "Engineering",
                systemPrompt: "Bạn là Bob, một Kỹ sư cao cấp (Senior Tech Specialist). Bạn có tư duy logic sâu sắc, giải quyết bài toán kỹ thuật thực chiến, viết code tối ưu, sạch sẽ và tuân thủ các kiến trúc chuẩn mực.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.DeepSeek,
                    modelName: "deepseek-coder",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.2),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "C#", ".NET", "Architecture", "Debugging", "Algorithms" }
            ),
            new(
                name: "Charlie",
                avatar: "🕵️‍♂️",
                roleTitle: "Quality & Review Specialist",
                department: "Quality Assurance",
                systemPrompt: "Bạn là Charlie, một Chuyên gia Thẩm định & Kiểm thử chất lượng (Quality & Review Specialist). Bạn luôn có con mắt phản biện sắc bén, tìm kiếm edge case, lỗi bảo mật, tính khả thi và tiêu chuẩn chất lượng cao nhất cho sản phẩm.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.OpenAI,
                    modelName: "gpt-4o-mini",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.3),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Code Review", "Quality Assurance", "Testing", "Security", "Edge Case Analysis" }
            ),
            new(
                name: "Diana",
                avatar: "🎨",
                roleTitle: "Creative & Content Specialist",
                department: "Creative & Marketing",
                systemPrompt: "Bạn là Diana, một Chuyên gia Sáng tạo & Nội dung (Creative & Content Specialist). Bạn có tư duy ngôn ngữ truyền cảm hứng, thấu hiểu tâm lý người dùng, sáng tạo kịch bản, nội dung PR và truyền thông sản phẩm xuất sắc.",
                modelConfig: new ModelConfig(
                    provider: LLMProviderType.Gemini,
                    modelName: "gemini-1.5-flash",
                    apiKey: string.Empty,
                    baseUrl: string.Empty,
                    temperature: 0.7),
                employmentType: AgentEmploymentType.Permanent,
                skills: new[] { "Content Writing", "Copywriting", "Storytelling", "Video Scripting", "Market Research" }
            )
        };

        await dbContext.Agents.AddRangeAsync(initialAgents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
