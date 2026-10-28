using FluentValidation;

namespace API.Application.Features.Agents.Commands.UpdateConfig;

public sealed class UpdateAgentConfigValidator : AbstractValidator<UpdateAgentConfigCommand>
{
    public UpdateAgentConfigValidator()
    {
        RuleFor(x => x.Request.AgentId)
            .NotEmpty()
            .WithMessage("Id nhân sự không được để trống.");

        RuleFor(x => x.Request.ModelName)
            .NotEmpty()
            .WithMessage("Tên model không được để trống.");
    }
}
