using System.Reflection;
using API.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace API.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            configuration.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
