using API.Application.Abstractions;
using API.Domain.Common;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class MockLLMProvider : ILLMProvider
{
    public string ProviderName { get; }

    public MockLLMProvider(string providerName = "Mock")
    {
        ProviderName = string.IsNullOrWhiteSpace(providerName) ? "Mock" : providerName;
    }

    public Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        string response;

        if (userPrompt.Contains("RecommendedModel", StringComparison.OrdinalIgnoreCase) ||
            userPrompt.Contains("Selene", StringComparison.OrdinalIgnoreCase))
        {
            response = """
                {
                  "RecommendedProvider": "DeepSeek",
                  "RecommendedModel": "deepseek-reasoner",
                  "ModelBenchmarkRationale": "Vượt trội về khả năng suy luận logic chuyên sâu với chi phí tối ưu",
                  "DraftSystemPrompt": "Bạn là chuyên gia được tuyển chọn để giải quyết bài toán chuyên môn."
                }
                """;
        }
        else if (userPrompt.Contains("NeedsHiring", StringComparison.OrdinalIgnoreCase) ||
                 systemPrompt.Contains("Minerva", StringComparison.OrdinalIgnoreCase))
        {
            response = """
                {
                  "ProposedPrimaryExecutor": "Specialist",
                  "ProposedTester": "Valeria",
                  "NeedsHiring": true,
                  "MissingRole": "Specialized Solutions Engineer",
                  "MissingSkill": "Advanced Domain Engineering",
                  "Reasoning": "Quy trình thực thi đòi hỏi kỹ năng chuyên môn sâu cần tuyển thêm nhân sự dự án."
                }
                """;
        }
        else if (systemPrompt.Contains("Freya", StringComparison.OrdinalIgnoreCase))
        {
            response = "Tôi đã nghiên cứu kỹ đề xuất ban đầu từ Athena. Dưới góc độ phân tích logic, tôi chỉ ra 2 điểm cần làm rõ: điều kiện ràng buộc dữ liệu đầu vào và các trường hợp ngoại lệ (edge cases) chưa được bao quát. Tôi đề nghị hội đồng bổ sung tiêu chí xác thực chặt chẽ hơn.";
        }
        else if (systemPrompt.Contains("Hera", StringComparison.OrdinalIgnoreCase))
        {
            response = "Xem xét quan điểm của cả Athena và Freya, tôi chất vấn các giả định lạc quan về thời gian hoàn thành. Tôi đề xuất dung hòa bằng cách giữ mục tiêu cốt lõi của Athena nhưng tích hợp toàn bộ ràng buộc kỹ thuật của Freya, tạo thành bản phân tích hoàn chỉnh và khả thi nhất.";
        }
        else if (systemPrompt.Contains("Themis", StringComparison.OrdinalIgnoreCase))
        {
            response = "Đối chiếu quy trình tuần tự của Serena với năng lực nội bộ, tôi phát hiện các mắt xích kỹ thuật cao đòi hỏi chuyên môn sâu mà đội ngũ hiện tại chưa tối ưu. Tôi đề xuất chuyển giao cho Phòng Tuyển dụng AI khảo sát ứng viên chuyên trách.";
        }
        else if (systemPrompt.Contains("Iris", StringComparison.OrdinalIgnoreCase))
        {
            response = "Khảo sát các benchmark kỹ thuật hàng đầu (HumanEval, MMLU, GSM8K), đối với kỹ năng chuyên sâu này, DeepSeek-R1 (deepseek-reasoner) và Claude 3.5 Sonnet đạt điểm số vượt trội trong phân tích logic và viết mã chính xác.";
        }
        else if (systemPrompt.Contains("Clio", StringComparison.OrdinalIgnoreCase))
        {
            response = "Dựa trên so sánh benchmark từ Iris, tôi đã thiết kế System Prompt đo ni đóng giày cho nhân sự mới, thiết lập ranh giới nghiệp vụ nghiêm ngặt để đảm bảo chất lượng đầu ra đạt chuẩn cao nhất.";
        }
        else if (systemPrompt.Contains("Astrid", StringComparison.OrdinalIgnoreCase))
        {
            response = "Tôi đã đọc nhận xét ban đầu của Valeria. Tôi tập trung kiểm thử độ bền vững (robustness), rà soát các tình huống quá tải và dữ liệu biên, xác nhận sản phẩm vận hành ổn định và không phát hiện lỗi nghiêm trọng.";
        }
        else if (systemPrompt.Contains("Diana", StringComparison.OrdinalIgnoreCase))
        {
            response = "Tổng hợp đánh giá từ Valeria và Astrid, tôi xác nhận sản phẩm đáp ứng xuất sắc kỳ vọng thực tế của người dùng, giao diện trực quan và giá trị ứng dụng cao. Tôi đồng thuận nghiệm thu và kính chuyển Người dùng phê duyệt.";
        }
        else
        {
            response = $"[Mô phỏng phản hồi từ {config.Provider} - {config.ModelName}]: Tôi đã tiếp nhận ý kiến của các đồng nghiệp và hoàn thành phần phân tích chuyên môn.";
        }

        return Task.FromResult(response);
    }

    public Task<Result<bool>> ValidateConnectionAsync(
        ModelConfig config,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
