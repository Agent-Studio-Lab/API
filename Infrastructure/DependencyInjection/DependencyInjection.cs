using API.Application.Abstractions;
using API.Infrastructure.BackgroundJobs;
using API.Infrastructure.LLM;
using API.Infrastructure.Persistence;
using API.Infrastructure.Persistence.Repositories;
using API.Infrastructure.Realtime;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("AgentStudioDb");
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            dbConnectionString = "Host=localhost;Port=5432;Database=AgentStudio;Username=postgres;Password=postgres";
        }

        // 1. Persistence & DbContext
        services.AddDbContext<AgentStudioDbContext>(options =>
        {
            options.UseNpgsql(dbConnectionString);
        });

        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<AgentStudioDbContext>());
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<ITaskTicketRepository, TaskTicketRepository>();

        // 2. Realtime
        services.AddSignalR();
        services.AddScoped<IOfficeEventPublisher, SignalREventPublisher>();

        // 3. LLM Providers & Factory
        services.AddHttpClient();
        services.AddScoped<ILLMProviderFactory, LLMProviderFactory>();

        // 4. Hangfire Background Jobs
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(dbConnectionString));
        });

        services.AddHangfireServer();
        services.AddScoped<TaskWorkflowJob>();
        services.AddScoped<ITaskWorkflowQueue, HangfireTaskWorkflowQueue>();

        return services;
    }
}
