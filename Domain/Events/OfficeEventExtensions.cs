using API.Domain.Common;
using API.Domain.Enums;

namespace API.Domain.Events;

public static class OfficeEventExtensions
{
    public static OfficeEvent CreateHiringRequested(
        string roleTitle,
        string department,
        string requiredSkill,
        string justification,
        string? recommendedProvider = null,
        string? recommendedModel = null,
        string? modelBenchmarkRationale = null)
    {
        var modelInfo = !string.IsNullOrWhiteSpace(recommendedModel)
            ? $" Phòng Đánh giá AI đề xuất cấu hình: {recommendedProvider} ({recommendedModel}). Lý do: {modelBenchmarkRationale}."
            : string.Empty;

        return new OfficeEvent(
            id: Guid.NewGuid(),
            eventType: OfficeEventType.AgentSpeaking,
            agentName: "Iris",
            agentAvatar: null,
            targetAgentName: "User",
            phase: TaskPhase.WaitingHiringApproval,
            message: $"Phòng Tuyển dụng & Thẩm định AI đề xuất tuyển dụng nhân sự dự án cho vai trò '{roleTitle}'. Kỹ năng: {requiredSkill}.{modelInfo} Kính mời Người dùng phê duyệt.",
            payload: new
            {
                roleTitle,
                department,
                requiredSkill,
                justification,
                recommendedProvider,
                recommendedModel,
                modelBenchmarkRationale
            },
            timestamp: Clock.Now);
    }

    public static OfficeEvent CreateWaitingFinalApproval(Guid ticketId, string title)
    {
        return new OfficeEvent(
            id: Guid.NewGuid(),
            eventType: OfficeEventType.AgentSpeaking,
            agentName: "Valeria",
            agentAvatar: null,
            targetAgentName: "User",
            phase: TaskPhase.WaitingFinalApproval,
            message: "Phòng Thẩm định đã kiểm thử và xác nhận sản phẩm đạt chuẩn chất lượng. Kính mời Người dùng nghiệm thu sản phẩm.",
            payload: new
            {
                ticketId,
                title
            },
            timestamp: Clock.Now);
    }
}
