using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ResourceGenerationService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _services;

    public ResourceGenerationService(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(UpdateResources, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Changed to 5 seconds
        return Task.CompletedTask;
    }

    private void UpdateResources(object state)
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var planets = context.Planets.Include(p => p.Buildings).ToList();
            foreach (var planet in planets)
            {
                planet.Iron += planet.RelativeIronOutput;
                planet.Silver += planet.RelativeSilverOutput;
                planet.Aluminium += planet.RelativeAluminiumOutput;
                planet.H2 += planet.RelativeH2Output;
                planet.Energy += planet.RelativeEnergyOutput;

                // Add resource output based on buildings
                foreach (var building in planet.Buildings)
                {
                    if (building.Name == "HQ")
                    {
                        planet.Iron += planet.RelativeIronOutput * building.Level;
                        planet.Silver += planet.RelativeSilverOutput * building.Level;
                        planet.Aluminium += planet.RelativeAluminiumOutput * building.Level;
                        planet.H2 += planet.RelativeH2Output * building.Level;
                        planet.Energy += planet.RelativeEnergyOutput * building.Level;
                    }
                    // Add other buildings here
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
