using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StellarIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
    public IActionResult Build(int planetId)
    {
        var buildingTypes = GetBuildingTypes();
        if (buildingTypes == null || !buildingTypes.Any())
        {
            _logger.LogWarning("No building types found.");
            return NotFound("No building types found.");
        }

        var model = new BuildViewModel
        {
            PlanetId = planetId,
            AvailableBuildings = buildingTypes.Select(b => new BuildingOption
            {
                Name = b.Name,
                Duration = 10, // Set duration to a default value, replace it with actual duration if available
                IronCost = b.IronCost,
                SilverCost = b.SilverCost,
                AluminiumCost = b.AluminiumCost,
                H2Cost = b.H2Cost,
                EnergyCost = b.EnergyCost
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
            var planet = await _context.Planets.FindAsync(model.PlanetId);
            if (planet == null)
            {
                _logger.LogWarning("Planet not found.");
                return NotFound("Planet not found.");
            }

            var selectedBuilding = model.AvailableBuildings.FirstOrDefault(b => b.Name == model.SelectedBuilding);
            if (selectedBuilding == null)
            {
                _logger.LogWarning("Selected building type not found.");
                return NotFound("Selected building type not found.");
            }

            var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == model.SelectedBuilding);

            if (existingBuilding != null)
            {
                _logger.LogInformation("Upgrading existing building.");
                existingBuilding.Level++;
                existingBuilding.ConstructionEndTime = DateTime.UtcNow.AddSeconds(selectedBuilding.Duration);
            }
            else
            {
                _logger.LogInformation("Adding new building.");
                var building = new Building
                {
                    Name = model.SelectedBuilding,
                    PlanetId = model.PlanetId,
                    IronCost = selectedBuilding.IronCost,
                    SilverCost = selectedBuilding.SilverCost,
                    AluminiumCost = selectedBuilding.AluminiumCost,
                    H2Cost = selectedBuilding.H2Cost,
                    EnergyCost = selectedBuilding.EnergyCost,
                    Level = 1,
                    ConstructionEndTime = DateTime.UtcNow.AddSeconds(selectedBuilding.Duration)
                };

                // Deduct resources from the planet
                planet.Iron -= selectedBuilding.IronCost;
                planet.Silver -= selectedBuilding.SilverCost;
                planet.Aluminium -= selectedBuilding.AluminiumCost;
                planet.H2 -= selectedBuilding.H2Cost;
                planet.Energy -= selectedBuilding.EnergyCost;

                _context.Buildings.Add(building);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "Home");
        }
        else
        {
            _logger.LogWarning("Model state is invalid.");
            foreach (var error in ModelState)
            {
                _logger.LogWarning($"{error.Key}: {error.Value.Errors.FirstOrDefault()?.ErrorMessage}");
            }
        }

        model.AvailableBuildings = GetBuildingTypes().Select(b => new BuildingOption
        {
            Name = b.Name,
            Duration = 10, // Set duration to a default value, replace it with actual duration if available
            IronCost = b.IronCost,
            SilverCost = b.SilverCost,
            AluminiumCost = b.AluminiumCost,
            H2Cost = b.H2Cost,
            EnergyCost = b.EnergyCost
        }).ToList();

        return View(model);
    }

    private List<Building> GetBuildingTypes()
    {
        return new List<Building>
        {
            new Building { Name = "HQ", IronCost = 300, SilverCost = 100, AluminiumCost = 150, H2Cost = 90, EnergyCost = 100 },
            new Building { Name = "Iron Mine", IronCost = 100, SilverCost = 500, AluminiumCost = 150, H2Cost = 140, EnergyCost = 140 },
            new Building { Name = "Silver Mine", IronCost = 100, SilverCost = 200, AluminiumCost = 75, H2Cost = 30, EnergyCost = 30 },
            new Building { Name = "Aluminum Workshop", IronCost = 300, SilverCost = 100, AluminiumCost = 50, H2Cost = 500, EnergyCost = 500 },
            new Building { Name = "H2 Condenser", IronCost = 350, SilverCost = 600, AluminiumCost = 50, H2Cost = 400, EnergyCost = 400 },
            new Building { Name = "Fusionsreactor", IronCost = 800, SilverCost = 900, AluminiumCost = 750, H2Cost = 50, EnergyCost = 50 },
            new Building { Name = "Research Center", IronCost = 300, SilverCost = 100, AluminiumCost = 150, H2Cost = 90, EnergyCost = 90 },
            new Building { Name = "Shipyard", IronCost = 300, SilverCost = 100, AluminiumCost = 150, H2Cost = 90, EnergyCost = 90 }
        };
    }
}
