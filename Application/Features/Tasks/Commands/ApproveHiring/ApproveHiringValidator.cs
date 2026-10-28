using FluentValidation;

namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed class ApproveHiringValidator : AbstractValidator<ApproveHiringCommand>
{
    public ApproveHiringValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("Id nhiệm vụ không được để trống.");

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Tên nhân sự dự án không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên nhân sự không vượt quá 100 ký tự.");

        RuleFor(x => x.Request.ModelName)
            .NotEmpty()
            .WithMessage("Tên model không được để trống.");
    }
}
