using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StellarIO.Models;
using StellarIO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BuildingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BuildService _buildService;
    private readonly PlanetService _planetService;
    private readonly ILogger<BuildingController> _logger;

    public BuildingController(ApplicationDbContext context, BuildService buildService, PlanetService planetService, ILogger<BuildingController> logger)
    {
        _context = context;
        _buildService = buildService;
        _planetService = planetService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Build(int planetId)
    {
        var planet = await _planetService.GetPlanetAsync(planetId);

        if (planet == null)
        {
            _logger.LogWarning("Planet not found.");
            return NotFound("Planet not found.");
        }
        var model = new BuildViewModel
        {
            PlanetId = planetId,
            PlanetCoordinates = planet.Coordinates, // Set this property
            PlanetIron = planet.Iron,
            PlanetSilver = planet.Silver,
            PlanetAluminium = planet.Aluminium,
            PlanetH2 = planet.H2,
            PlanetEnergy = planet.Energy,
            AvailableBuildings = PlanetService.GetBuildingOptions(planet)
        };

        return View(model);
    }


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Build(BuildViewModel model)
    {
        _logger.LogInformation("Build POST method called.");

        // Remove the validation for PlanetCoordinates if it's not necessary
        ModelState.Remove("PlanetCoordinates");

        if (ModelState.IsValid)
        {
            var planet = await _planetService.GetPlanetAsync(model.PlanetId);
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

            var selectedBuildingType = BuildService.GetBuildingTypes().FirstOrDefault(b => b.Name == model.SelectedBuilding);
            if (selectedBuildingType == null)
            {
                _logger.LogWarning("Building type not found.");
                return NotFound("Building type not found.");
            }

            if (!BuildService.CheckBuildingRequirements(planet, selectedBuildingType.Name))
            {
                _logger.LogWarning("Building requirements not met.");
                ModelState.AddModelError("", "Building requirements not met.");
                return View(model);
            }

            var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == model.SelectedBuilding);
            int level = existingBuilding != null ? existingBuilding.Level + 1 : 1;

            // Recalculate costs based on building level
            var recalculatedCosts = BuildService.RecalculateCosts(selectedBuildingType, level);

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
                    ConstructionEndTime = DateTime.UtcNow.AddSeconds(recalculatedCosts.Duration),
                    Description = selectedBuildingType.Description
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
        try
        {
            await _buildService.CancelBuildingAsync(buildingId);
            return RedirectToAction("Dashboard", "Home");
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (BadHttpRequestException e)
        {
            return BadRequest(e.Message);
        }
    }
}
