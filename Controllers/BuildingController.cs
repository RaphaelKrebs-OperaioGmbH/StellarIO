using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StellarIO.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class BuildingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BuildingController> _logger;

    public BuildingController(ApplicationDbContext context, ILogger<BuildingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Build(int planetId)
    {
        var buildingTypes = GetBuildingTypes();
        if (buildingTypes == null || !buildingTypes.Any())
        {
            return NotFound("No building types found.");
        }

        var planet = await _context.Planets.Include(p => p.Buildings).FirstOrDefaultAsync(p => p.Id == planetId);
        if (planet == null)
        {
            _logger.LogWarning("Planet not found.");
            return NotFound("Planet not found.");
        }

        var model = new BuildViewModel
        {
            PlanetId = planetId,
            AvailableBuildings = buildingTypes.Select(bt =>
            {
                var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == bt.Name);
                int level = existingBuilding != null ? existingBuilding.Level + 1 : 1;

                var recalculatedCosts = RecalculateCosts(bt, level);

                return new BuildingOption
                {
                    Name = bt.Name,
                    Duration = recalculatedCosts.Duration,
                    IronCost = recalculatedCosts.IronCost,
                    SilverCost = recalculatedCosts.SilverCost,
                    AluminiumCost = recalculatedCosts.AluminiumCost,
                    H2Cost = recalculatedCosts.H2Cost,
                    EnergyCost = recalculatedCosts.EnergyCost
                };
            }).ToList()
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Build(BuildViewModel model)
    {
        _logger.LogInformation("Build POST method called.");

        if (ModelState.IsValid)
        {
            var planet = await _context.Planets.Include(p => p.Buildings).FirstOrDefaultAsync(p => p.Id == model.PlanetId);
            if (planet == null)
            {
                _logger.LogWarning("Planet not found.");
                return NotFound("Planet not found.");
            }

            // Check if there's already a building in progress
            if (planet.Buildings.Any(b => b.ConstructionEndTime > DateTime.UtcNow))
            {
                _logger.LogWarning("A building is already in progress.");
                ModelState.AddModelError("", "A building is already in progress on this planet.");
                return View(model);
            }

            var selectedBuildingType = GetBuildingTypes().FirstOrDefault(b => b.Name == model.SelectedBuilding);
            if (selectedBuildingType == null)
            {
                _logger.LogWarning("Building type not found.");
                return NotFound("Building type not found.");
            }

            var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == model.SelectedBuilding);
            int level = existingBuilding != null ? existingBuilding.Level + 1 : 1;

            // Recalculate costs based on building level
            var recalculatedCosts = RecalculateCosts(selectedBuildingType, level);

            // Check if the planet has enough resources
            if (planet.Iron < recalculatedCosts.IronCost ||
                planet.Silver < recalculatedCosts.SilverCost ||
                planet.Aluminium < recalculatedCosts.AluminiumCost ||
                planet.H2 < recalculatedCosts.H2Cost ||
                planet.Energy < recalculatedCosts.EnergyCost)
            {
                _logger.LogWarning("Not enough resources to build this building.");
                ModelState.AddModelError("", "Not enough resources to build this building.");
                return View(model);
            }

            // Deduct resources from the planet
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
                var building = new Building
                {
                    Name = model.SelectedBuilding,
                    PlanetId = model.PlanetId,
                    IronCost = recalculatedCosts.IronCost,
                    SilverCost = recalculatedCosts.SilverCost,
                    AluminiumCost = recalculatedCosts.AluminiumCost,
                    H2Cost = recalculatedCosts.H2Cost,
                    EnergyCost = recalculatedCosts.EnergyCost,
                    Level = 1,
                    ConstructionEndTime = DateTime.UtcNow.AddSeconds(recalculatedCosts.Duration)
                };

                _context.Buildings.Add(building);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Building added/updated successfully.");

            return RedirectToAction("Dashboard", "Home");
        }
        else
        {
            _logger.LogWarning("Model state is invalid.");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"{state.Key}: {error.ErrorMessage}");
                }
            }
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Cancel(int buildingId)
    {
        var building = await _context.Buildings.Include(b => b.Planet).FirstOrDefaultAsync(b => b.Id == buildingId);
        if (building == null)
        {
            return NotFound("Building not found.");
        }

        var planet = building.Planet;
        if (planet == null)
        {
            return NotFound("Planet not found.");
        }

        // Check if the building is currently under construction
        if (building.ConstructionEndTime > DateTime.UtcNow)
        {
            if (building.Level == 1)
            {
                // If the building is new and has not completed its first level, remove it
                _context.Buildings.Remove(building);
            }
            else
            {
                // If the building is being upgraded, just cancel the upgrade
                building.Level--;
                building.ConstructionEndTime = null;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Dashboard", "Home");
    }

    private List<Building> GetBuildingTypes()
    {
        return new List<Building>
        {
            new Building { Name = "HQ", IronCost = 300, SilverCost = 100, AluminiumCost = 150, H2Cost = 90, EnergyCost = 100, Duration = 10 },
            new Building { Name = "Iron Mine", IronCost = 100, SilverCost = 500, AluminiumCost = 150, H2Cost = 140, EnergyCost = 140, Duration = 60 },
            new Building { Name = "Silver Mine", IronCost = 100, SilverCost = 200, AluminiumCost = 75, H2Cost = 30, EnergyCost = 30, Duration = 60 },
            new Building { Name = "Aluminum Mill", IronCost = 300, SilverCost = 100, AluminiumCost = 50, H2Cost = 500, EnergyCost = 500, Duration = 60 },
            new Building { Name = "H2 Condenser", IronCost = 350, SilverCost = 600, AluminiumCost = 50, H2Cost = 400, EnergyCost = 400, Duration = 60 },
            new Building { Name = "Fusion Reactor", IronCost = 800, SilverCost = 900, AluminiumCost = 750, H2Cost = 50, EnergyCost = 50, Duration = 60 },
            new Building { Name = "Research Center", IronCost = 300, SilverCost = 100, AluminiumCost = 150, H2Cost = 90, EnergyCost = 90, Duration = 60 },
            new Building { Name = "Shipyard", IronCost = 18000, SilverCost = 10000, AluminiumCost = 15000, H2Cost = 9000, EnergyCost = 9000, Duration = 60 }
        };
    }

    private Building RecalculateCosts(Building buildingType, int level)
    {
        return new Building
        {
            IronCost = buildingType.IronCost * level,
            SilverCost = buildingType.SilverCost * level,
            AluminiumCost = buildingType.AluminiumCost * level,
            H2Cost = buildingType.H2Cost * level,
            EnergyCost = buildingType.EnergyCost * level,
            Duration = buildingType.Duration * level
        };
    }
}
