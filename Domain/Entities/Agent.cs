using API.Domain.Common;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Domain.Entities;

public sealed class Agent : AggregateRoot
{
    public string Name { get; private set; }
    public string Avatar { get; private set; }
    public AgentRole Role { get; private set; }
    public string RoleTitle { get; private set; }
    public string Department { get; private set; }
    public string SystemPrompt { get; private set; }
    public ModelConfig ModelConfig { get; private set; }
    public string Status { get; private set; }

    private readonly List<OfficeMessage> _memory = [];
    public IReadOnlyCollection<OfficeMessage> Memory => _memory.AsReadOnly();

    public Agent(
        string name,
        string avatar,
        AgentRole role,
        string roleTitle,
        string department,
        string systemPrompt,
        ModelConfig modelConfig,
        Guid? id = null) : base(id)
    {
        Name = name;
        Avatar = avatar;
        Role = role;
        RoleTitle = roleTitle;
        Department = department;
        SystemPrompt = systemPrompt;
        ModelConfig = modelConfig;
        Status = string.Empty;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }

    public void UpdateModelConfig(ModelConfig modelConfig)
    {
        ModelConfig = modelConfig;
    }

    public void AddMemory(OfficeMessage message)
    {
        _memory.Add(message);
    }

    public void ClearMemory()
    {
        _memory.Clear();
    }
}
