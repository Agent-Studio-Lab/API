using API.Domain.Constants;
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

        RuleFor(x => x.Request.BaseUrl)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Provider) && !LLMProviders.IsKnownProvider(x.Request.Provider))
            .WithMessage(x => $"Nhà cung cấp '{x.Request.Provider}' là tùy biến/chưa có trong danh sách định sẵn. Bạn phải cung cấp Base URL hợp lệ.");

        RuleFor(x => x.Request.BaseUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.BaseUrl))
            .WithMessage("Base URL phải là địa chỉ URL hợp lệ bắt đầu bằng http:// hoặc https://.");

        RuleFor(x => x.Request.ApiKey)
            .NotEmpty()
            .When(x => !LLMProviders.IsLocalOrMock(x.Request.Provider))
            .WithMessage("API Key không được để trống đối với nhà cung cấp này.");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
