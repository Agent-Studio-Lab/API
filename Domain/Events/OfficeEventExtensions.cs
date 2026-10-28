using API.Domain.Common;
using API.Domain.Enums;

namespace API.Domain.Events;

public static class OfficeEventExtensions
{
    public static OfficeEvent CreateHiringRequested(string roleTitle, string department, string requiredSkill, string justification)
    {
        return new OfficeEvent(
            id: Guid.NewGuid(),
            eventType: OfficeEventType.AgentSpeaking,
            agentName: "Themis",
            agentAvatar: "⚖️",
            targetAgentName: "User",
            phase: TaskPhase.WaitingHiringApproval,
            message: $"Phòng Kế hoạch đề xuất tuyển dụng nhân sự dự án cho vai trò '{roleTitle}' thuộc phòng ban '{department}'. Lý do: {justification}. Kính mời Người dùng phê duyệt.",
            payload: new
            {
                roleTitle,
                department,
                requiredSkill,
                justification
            },
            timestamp: Clock.Now);
    }

    public static OfficeEvent CreateWaitingFinalApproval(Guid ticketId, string title)
    {
        return new OfficeEvent(
            id: Guid.NewGuid(),
            eventType: OfficeEventType.AgentSpeaking,
            agentName: "Valeria",
            agentAvatar: "🔍",
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
