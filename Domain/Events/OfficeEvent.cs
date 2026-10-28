using API.Domain.Common;
using API.Domain.Enums;

namespace API.Domain.Events;

public sealed class OfficeEvent
{
    public Guid Id { get; }
    public OfficeEventType EventType { get; }
    public string? AgentName { get; }
    public string? AgentAvatar { get; }
    public string? TargetAgentName { get; }
    public TaskPhase Phase { get; }
    public string Message { get; }
    public object? Payload { get; }
    public DateTime Timestamp { get; }

    public OfficeEvent(
        Guid id,
        OfficeEventType eventType,
        string? agentName,
        string? agentAvatar,
        string? targetAgentName,
        TaskPhase phase,
        string message,
        object? payload,
        DateTime timestamp)
    {
        Id = id;
        EventType = eventType;
        AgentName = agentName;
        AgentAvatar = agentAvatar;
        TargetAgentName = targetAgentName;
        Phase = phase;
        Message = message;
        Payload = payload;
        Timestamp = timestamp;
    }

    public static OfficeEvent CreateThinking(string agentName, string? avatar, TaskPhase phase, string thought) =>
        new(Guid.NewGuid(), OfficeEventType.AgentThinking, agentName, avatar, null, phase, thought, null, Clock.Now);

    public static OfficeEvent CreateSpeaking(string agentName, string? avatar, string? targetAgent, TaskPhase phase, string text, object? payload = null) =>
        new(Guid.NewGuid(), OfficeEventType.AgentSpeaking, agentName, avatar, targetAgent, phase, text, payload, Clock.Now);

    public static OfficeEvent CreatePhaseChanged(TaskPhase newPhase, string description) =>
        new(Guid.NewGuid(), OfficeEventType.TaskPhaseChanged, null, null, null, newPhase, description, null, Clock.Now);

    public static OfficeEvent CreateCompleted(string summary, object? finalArtifact = null) =>
        new(Guid.NewGuid(), OfficeEventType.TaskCompleted, null, null, null, TaskPhase.Completed, summary, finalArtifact, Clock.Now);
}
