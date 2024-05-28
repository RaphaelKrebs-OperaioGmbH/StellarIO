using Microsoft.EntityFrameworkCore;
using StellarIO.Models;

namespace StellarIO.Services
{
    public class PlanetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlanetService> _logger;

        public PlanetService(ApplicationDbContext context, ILogger<PlanetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual IQueryable<Planet> GetAllPlanets()
        {
            return _context.Planets
                .Include(p => p.Buildings)
                .Include(p => p.System)
                .ThenInclude(s => s.Galaxy)
                .AsQueryable();
        }

        public virtual IQueryable<Planet> GetPlanetsOwnedByUser(string userId)
        {
            var planets = GetAllPlanets();
            return planets.Where(p => p.UserId == userId);
        }

        public virtual async Task<Planet?> GetPlanetAsync(int planetId)
        {
            return await GetAllPlanets().Where(p => p.Id == planetId).FirstOrDefaultAsync();
        }

        public static List<BuildingOption> GetBuildingOptions(Planet planet)
        {
            var buildingTypes = BuildService.GetBuildingTypes();
            if (buildingTypes == null)
            {
                throw new Exception("No Building Types could be found - this is a configuration issue...");
            }
            
            var availableBuildingOptions = buildingTypes.Where(bt => BuildService.CheckBuildingRequirements(planet, bt.Name))
                .Select(bt =>
                {
                    var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == bt.Name);
                    int level = existingBuilding != null ? existingBuilding.Level + 1 : 1;

                    var recalculatedCosts = BuildService.RecalculateCosts(bt, level);

                    return new BuildingOption
                    {
                        Name = bt.Name,
                        Duration = recalculatedCosts.Duration,
                        IronCost = recalculatedCosts.IronCost,
                        SilverCost = recalculatedCosts.SilverCost,
                        AluminiumCost = recalculatedCosts.AluminiumCost,
                        H2Cost = recalculatedCosts.H2Cost,
                        EnergyCost = recalculatedCosts.EnergyCost,
                        Level = level,
                        Description = bt.Description // Pass Description
                    };
                });
            return availableBuildingOptions.ToList();
        }

        public virtual async Task<List<BuildingOption>> GetBuildingOptionsAsync(int planetId)
        {
            var planet = await GetPlanetAsync(planetId);
            if (planet == null)
            {
                throw new KeyNotFoundException($"No Planet with id {planetId} could be found to get the building options for.");
            }
            return GetBuildingOptions(planet);
        }
    }
}
