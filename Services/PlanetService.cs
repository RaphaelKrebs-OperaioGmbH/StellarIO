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

        public virtual async Task ConstructBuildingAsync (int planetId, string buildingName)
        {
            var planet = await GetPlanetAsync(planetId);
            if (planet == null)
            {
                throw new KeyNotFoundException($"No Planet with id {planetId} could be found to start building construction on");
            }

            if(planet.Buildings.Any(b => b.ConstructionEndTime > DateTime.UtcNow))
            {
                throw new BadHttpRequestException($"A construction is already in progress on the Planet with id {planetId}. Please wait until this construction is finished, or cancel construction to start a new one.");
            }

            var selectedBuildingType = BuildService.GetBuildingTypes().FirstOrDefault(bt => bt.Name == buildingName);
            if (selectedBuildingType == null)
            {
                throw new KeyNotFoundException($"The building with name {buildingName} does not exists and can therefore not be constructed on the Planet with id {planetId}");
            }

            if(!BuildService.CheckBuildingRequirements(planet, buildingName))
            {
                throw new BadHttpRequestException($"Building requirements not met for starting the construction of {buildingName} on Planet with Id {planetId}");
            }

            var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == buildingName);
            int level = existingBuilding != null ? existingBuilding.Level + 1 : 1;

            var recalculatedCosts = BuildService.RecalculateCosts(selectedBuildingType, level);
            if (planet.Iron < recalculatedCosts.IronCost ||
                planet.Silver < recalculatedCosts.SilverCost ||
                planet.Aluminium < recalculatedCosts.AluminiumCost ||
                planet.H2 < recalculatedCosts.H2Cost ||
                planet.Energy < recalculatedCosts.EnergyCost)
            {
                throw new BadHttpRequestException($"Not enough resources to build {buildingName} on Planet with id {planetId}");
            }

            planet.Iron -= recalculatedCosts.IronCost;
            planet.Silver -= recalculatedCosts.SilverCost;
            planet.Aluminium -= recalculatedCosts.AluminiumCost;
            planet.H2 -= recalculatedCosts.H2Cost;
            planet.Energy -= recalculatedCosts.EnergyCost;

            if (existingBuilding != null)
            {
                existingBuilding.Level++;
                existingBuilding.ConstructionEndTime = DateTime.UtcNow.AddSeconds(recalculatedCosts.Duration);
            }

            else
            {
                planet.Buildings.Add(new Building
                {
                    Name = buildingName,
                    PlanetId = planet.Id,
                    IronCost = recalculatedCosts.IronCost,
                    SilverCost = recalculatedCosts.SilverCost,
                    AluminiumCost = recalculatedCosts.AluminiumCost,
                    H2Cost = recalculatedCosts.H2Cost,
                    EnergyCost = recalculatedCosts.EnergyCost,
                    Level = 1,
                    ConstructionEndTime = DateTime.UtcNow.AddSeconds(recalculatedCosts.Duration),
                    Description = selectedBuildingType.Description
                });
            }

            await _context.SaveChangesAsync();

        }

        public async Task AssignPlanetToNewUserAsync(User user)
        {
            _logger.LogInformation("Assigning a planet to the newly registered user {UserId}.", user.Id);
            var planet = await _context.Planets
                .Where(p => p.UserId == null)
                .FirstOrDefaultAsync();

            if (planet != null)
            {
                planet.UserId = user.Id;
                planet.User = user;
                _context.Planets.Update(planet);
                _logger.LogInformation("Updating planet {PlanetId} with new user {UserId}.", planet.Id, user.Id);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Planet {PlanetId} assigned to newly registered user {UserId}.", planet.Id, user.Id);
            }
            else
            {
                _logger.LogWarning("No unassigned planets available.");
                throw new InvalidOperationException("No unassigned planets available.");
            }
        }
    }
}
