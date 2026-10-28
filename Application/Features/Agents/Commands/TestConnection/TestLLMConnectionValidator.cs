using API.Domain.Enums;
using FluentValidation;

namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed class TestLLMConnectionValidator : AbstractValidator<TestLLMConnectionCommand>
{
    public TestLLMConnectionValidator()
    {
        RuleFor(x => x.Request.ModelName)
            .NotEmpty()
            .WithMessage("Tên Model không được để trống khi kiểm tra kết nối.");

        RuleFor(x => x.Request.ApiKey)
            .NotEmpty()
            .When(x => x.Request.Provider != LLMProviderType.Mock && x.Request.Provider != LLMProviderType.Ollama)
            .WithMessage("API Key không được để trống đối với nhà cung cấp này.");
    }
}
