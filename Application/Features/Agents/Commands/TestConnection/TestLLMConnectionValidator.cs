using FluentValidation;

namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed class TestLLMConnectionValidator : AbstractValidator<TestLLMConnectionCommand>
{
    public TestLLMConnectionValidator()
    {
        RuleFor(x => x.Request.Provider)
            .NotEmpty()
            .WithMessage("Nhà cung cấp (Provider) không được để trống khi kiểm tra kết nối.");

        RuleFor(x => x.Request.ModelName)
            .NotEmpty()
            .WithMessage("Tên Model không được để trống khi kiểm tra kết nối.");

        RuleFor(x => x.Request.ApiKey)
            .NotEmpty()
            .When(x => !IsLocalOrMock(x.Request.Provider))
            .WithMessage("API Key không được để trống đối với nhà cung cấp này.");
    }

    private static bool IsLocalOrMock(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return true;
        var p = provider.Trim().ToLowerInvariant();
        return p is "mock" or "ollama" or "local" or "custom" or "customopenai";
    }
}
