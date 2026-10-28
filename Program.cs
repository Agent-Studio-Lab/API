using API.Application.DependencyInjection;
using API.Infrastructure.DependencyInjection;
using API.Infrastructure.Persistence;
using API.Infrastructure.Persistence.Seeding;
using API.Infrastructure.Realtime;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// SignalR Hub Endpoint
app.MapHub<OfficeHub>("/officeHub");

app.MapControllers();

// Auto Migration & Data Seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AgentStudioDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(dbContext, builder.Configuration);
}

app.Run();
