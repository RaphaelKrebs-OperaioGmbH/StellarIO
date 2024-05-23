using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BuildingConstructionService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _services;

    public BuildingConstructionService(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(UpdateBuildings, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        return Task.CompletedTask;
    }

    private void UpdateBuildings(object state)
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var buildingsUnderConstruction = context.Buildings.Where(b => b.ConstructionEndTime > DateTime.UtcNow).ToList();
            foreach (var building in buildingsUnderConstruction)
            {
                if (DateTime.UtcNow >= building.ConstructionEndTime)
                {
                    building.ConstructionEndTime = null;
                    context.Buildings.Update(building);
                }
            }

            context.SaveChanges();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
