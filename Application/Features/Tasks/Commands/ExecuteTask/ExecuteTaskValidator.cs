using FluentValidation;

namespace API.Application.Features.Tasks.Commands.ExecuteTask;

public sealed class ExecuteTaskValidator : AbstractValidator<ExecuteTaskCommand>
{
    public ExecuteTaskValidator()
    {
        RuleFor(x => x.Request.Prompt)
            .NotEmpty()
            .WithMessage("Đề bài không được để trống!")
            .MinimumLength(5)
            .WithMessage("Đề bài cần tối thiểu 5 ký tự.");

        RuleFor(x => x.Request.MaxFeedbackLoops)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Số vòng lặp tối đa phải lớn hơn hoặc bằng 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("Số vòng lặp tối đa không vượt quá 10.");
    }
}
