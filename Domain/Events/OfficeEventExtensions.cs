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
            targetAgentName: "Sếp (User)",
            phase: TaskPhase.WaitingHiringApproval,
            message: $"Báo cáo Sếp: Phòng Kế hoạch đã họp bàn và nhận thấy dự án này cần chuyên môn sâu về '{roleTitle}' thuộc phòng ban '{department}'. Hiện công ty chưa có nhân sự đáp ứng, kính đề nghị Sếp xem xét phê duyệt tuyển dụng!",
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
            targetAgentName: "Sếp (User)",
            phase: TaskPhase.WaitingFinalApproval,
            message: "Báo cáo Sếp: Phòng Thẩm định đã kiểm thử và nghiệm thu sản phẩm đạt chuẩn 100%. Kính mời Sếp duyệt nghiệm thu và quyết định nhân sự dự án!",
            payload: new
            {
                ticketId,
                title
            },
            timestamp: Clock.Now);
    }
}
