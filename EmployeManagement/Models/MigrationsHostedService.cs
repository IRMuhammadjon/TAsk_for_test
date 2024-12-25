using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeManagement.Models;


    public class MigrationsHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<MigrationsHostedService> logger) : IHostedService
    {
        private AppDbContext context = default!;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (configuration.GetValue<bool>("MigrateDatabase"))
            {
                logger.LogInformation("Migrating database.");
                await context.Database.MigrateAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
