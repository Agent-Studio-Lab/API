namespace API.Domain.ValueObjects;

public sealed class HiringProposal
{
    public string RoleTitle { get; }
    public string Department { get; }
    public string RequiredSkill { get; }
    public string Justification { get; }

    public HiringProposal(string roleTitle, string department, string requiredSkill, string justification)
    {
        RoleTitle = roleTitle;
        Department = department;
        RequiredSkill = requiredSkill;
        Justification = justification;
    }
}
