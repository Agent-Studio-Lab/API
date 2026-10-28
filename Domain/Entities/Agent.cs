using API.Domain.Common;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Domain.Entities;

public sealed class Agent : AggregateRoot
{
    public string Name { get; private set; }
    public string Avatar { get; private set; }
    public string RoleTitle { get; private set; }
    public string Department { get; private set; }
    public string SystemPrompt { get; private set; }
    public ModelConfig ModelConfig { get; private set; }
    public string Status { get; private set; }
    public AgentEmploymentType EmploymentType { get; private set; }

    private readonly List<string> _skills = [];
    public IReadOnlyCollection<string> Skills => _skills.AsReadOnly();

    private readonly List<OfficeMessage> _memory = [];
    public IReadOnlyCollection<OfficeMessage> Memory => _memory.AsReadOnly();

    public Agent(
        string name,
        string avatar,
        string roleTitle,
        string department,
        string systemPrompt,
        ModelConfig modelConfig,
        AgentEmploymentType employmentType,
        IEnumerable<string>? skills = null,
        Guid? id = null) : base(id)
    {
        Name = name;
        Avatar = avatar;
        RoleTitle = roleTitle;
        Department = department;
        SystemPrompt = systemPrompt;
        ModelConfig = modelConfig;
        EmploymentType = employmentType;
        Status = string.Empty;

        if (skills is not null)
        {
            _skills.AddRange(skills);
        }
    }

    public static Agent CreateContractor(
        string name,
        string avatar,
        HiringProposal proposal,
        ModelConfig modelConfig)
    {
        return new Agent(
            name: name,
            avatar: avatar,
            roleTitle: proposal.RoleTitle,
            department: proposal.Department,
            systemPrompt: $"Bạn là {name}, đảm nhiệm vai trò {proposal.RoleTitle}. Bạn có trách nhiệm giải quyết yêu cầu dự án theo đúng kỹ năng chuyên môn: {proposal.RequiredSkill}.",
            modelConfig: modelConfig,
            employmentType: AgentEmploymentType.Contractor,
            skills: new[] { proposal.RequiredSkill });
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }

    public void UpdateModelConfig(ModelConfig modelConfig)
    {
        ModelConfig = modelConfig;
    }

    public void PromoteToPermanent()
    {
        EmploymentType = AgentEmploymentType.Permanent;
    }

    public void DismissContractor()
    {
        if (EmploymentType != AgentEmploymentType.Contractor)
        {
            throw new InvalidOperationException("Chỉ có thể kết thúc hợp đồng đối với nhân sự dự án.");
        }

        SetDeleted(Clock.Now, "System");
    }

    public void AddSkill(string skill)
    {
        if (!string.IsNullOrWhiteSpace(skill) && !_skills.Contains(skill))
        {
            _skills.Add(skill);
        }
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
