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
            try
            {
                await _planetService.ConstructBuildingAsync(model.PlanetId, model.SelectedBuilding);
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
