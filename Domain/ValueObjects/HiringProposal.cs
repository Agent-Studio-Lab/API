namespace API.Domain.ValueObjects;

public sealed record HiringProposal(
    string RoleTitle,
    string Department,
    string RequiredSkill,
    string Justification,
    string? RecommendedProvider = null,
    string? RecommendedModel = null,
    string? ModelBenchmarkRationale = null,
    string? DraftSystemPrompt = null);
