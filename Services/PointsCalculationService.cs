using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using StellarIO.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class PointsCalculationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public PointsCalculationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CalculateAndUpdatePointsAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CalculateAndUpdatePointsAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Eagerly load related entities
            var users = context.Users
                .Include(u => u.Planets)
                .ThenInclude(p => p.Buildings)
                .ToList();

            foreach (var user in users)
            {
                int totalPoints = 0;

                foreach (var planet in user.Planets)
                {
                    foreach (var building in planet.Buildings)
                    {
                        if (building.ConstructionEndTime == null || building.ConstructionEndTime <= DateTime.UtcNow)
                        {
                            totalPoints += building.Points * building.Level;
                        }
                    }
                }

                user.Points = totalPoints;
            }

            await context.SaveChangesAsync();
        }
    }
}
