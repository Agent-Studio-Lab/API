namespace API.Domain.ValueObjects;

public sealed record HiringProposal(
    string RoleTitle,
    string Department,
    string RequiredSkill,
    string Justification);
