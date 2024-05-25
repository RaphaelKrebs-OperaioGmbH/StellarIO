using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StellarIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ScienceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ScienceController> _logger;

    public ScienceController(ApplicationDbContext context, UserManager<User> userManager, ILogger<ScienceController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        // Check if the user has a Research Center on any planet
        bool hasResearchCenter = await _context.Planets
            .Where(p => p.UserId == user.Id)
            .AnyAsync(p => p.Buildings.Any(b => b.Name == "Research Center" && (b.ConstructionEndTime == null || b.ConstructionEndTime <= DateTime.UtcNow)));

        if (!hasResearchCenter)
        {
            ViewData["Message"] = "You need a Research Center on at least one of your planets to access sciences.";
            return View(new ScienceViewModel());
        }

        var availableSciences = GetScienceTypes();

        var userSciences = await _context.Sciences
            .Where(s => s.UserId == user.Id)
            .ToListAsync();

        // Include the System property when loading planets
        var planetsWithResearchCenter = await _context.Planets
            .Where(p => p.UserId == user.Id && p.Buildings.Any(b => b.Name == "Research Center" && (b.ConstructionEndTime == null || b.ConstructionEndTime <= DateTime.UtcNow)))
            .Include(p => p.System)
            .ToListAsync();

        var selectedPlanet = planetsWithResearchCenter.FirstOrDefault();
        var planetSelectList = new SelectList(planetsWithResearchCenter, "Id", "Coordinates", selectedPlanet?.Id);

        var activeScience = await _context.Sciences.FirstOrDefaultAsync(s => s.Id == user.ActiveScienceId);

        // Check if the active research is completed
        if (activeScience != null && activeScience.ResearchEndTime <= DateTime.UtcNow)
        {
            // Increment the science level
            activeScience.Level++;
            activeScience.ResearchStartTime = null;
            activeScience.ResearchEndTime = null;
            user.ActiveScienceId = null;

            await _context.SaveChangesAsync();
            activeScience = null; // Clear the active science as it is completed
        }

        var model = new ScienceViewModel
        {
            AvailableSciences = availableSciences.Select(science =>
            {
                var userScience = userSciences.FirstOrDefault(us => us.Name == science.Name);
                return new ScienceOption
                {
                    Id = science.Id,
                    Name = science.Name,
                    Description = science.Description,
                    Level = userScience?.Level ?? 0,
                    IronCost = science.IronCost,
                    SilverCost = science.SilverCost,
                    AluminiumCost = science.AluminiumCost,
                    H2Cost = science.H2Cost,
                    EnergyCost = science.EnergyCost,
                    Duration = science.Duration
                };
            }).ToList(),
            ActiveScience = activeScience,
            PlanetSelectList = planetSelectList,
            SelectedPlanetId = selectedPlanet?.Id ?? 0,
            PlanetIron = selectedPlanet?.Iron ?? 0,
            PlanetSilver = selectedPlanet?.Silver ?? 0,
            PlanetAluminium = selectedPlanet?.Aluminium ?? 0,
            PlanetH2 = selectedPlanet?.H2 ?? 0,
            PlanetEnergy = selectedPlanet?.Energy ?? 0,
            PlanetCoordinates = selectedPlanet?.Coordinates
        };

        return View(model);
    }



    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Start(int scienceId, ScienceViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        // Ensure the user has an active science job to start
        if (user.ActiveScienceId.HasValue)
        {
            ModelState.AddModelError("", "You already have an active science job.");
            return RedirectToAction("Index");
        }

        // Get the base science data to determine the cost and duration
        var baseScience = GetScienceTypes().FirstOrDefault(s => s.Id == scienceId);
        if (baseScience == null)
        {
            return NotFound();
        }

        // Check if the user has enough resources and a completed Research Center
        var planetWithResources = await _context.Planets
            .Where(p => p.UserId == user.Id && p.Id == model.SelectedPlanetId)
            .Include(p => p.Buildings)
            .FirstOrDefaultAsync(p => p.Buildings.Any(b => b.Name == "Research Center" && (b.ConstructionEndTime == null || b.ConstructionEndTime <= DateTime.UtcNow)) &&
                                       p.Iron >= baseScience.IronCost &&
                                       p.Silver >= baseScience.SilverCost &&
                                       p.Aluminium >= baseScience.AluminiumCost &&
                                       p.H2 >= baseScience.H2Cost &&
                                       p.Energy >= baseScience.EnergyCost);

        if (planetWithResources == null)
        {
            ModelState.AddModelError("", "Not enough resources to start this science.");
            return RedirectToAction("Index");
        }

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Deduct resources from the planet
                planetWithResources.Iron -= baseScience.IronCost;
                planetWithResources.Silver -= baseScience.SilverCost;
                planetWithResources.Aluminium -= baseScience.AluminiumCost;
                planetWithResources.H2 -= baseScience.H2Cost;
                planetWithResources.Energy -= baseScience.EnergyCost;

                // Create a new Science instance for the user if not already existing
                var userScience = await _context.Sciences
                    .FirstOrDefaultAsync(s => s.Name == baseScience.Name && s.UserId == user.Id);

                if (userScience == null)
                {
                    userScience = new Science
                    {
                        Name = baseScience.Name,
                        UserId = user.Id,
                        Level = 0, // Starting level for a new science, will be incremented after research completes
                        IronCost = baseScience.IronCost,
                        SilverCost = baseScience.SilverCost,
                        AluminiumCost = baseScience.AluminiumCost,
                        H2Cost = baseScience.H2Cost,
                        EnergyCost = baseScience.EnergyCost,
                        Duration = baseScience.Duration,
                        Description = baseScience.Description
                    };
                    _context.Sciences.Add(userScience);
                    await _context.SaveChangesAsync(); // Ensure the new science is saved and its Id is set
                }

                // Set the active science
                user.ActiveScienceId = userScience.Id;
                userScience.ResearchStartTime = DateTime.UtcNow;
                userScience.ResearchEndTime = DateTime.UtcNow.AddSeconds(baseScience.Duration);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Cancel()
    {
        var user = await _userManager.GetUserAsync(User);
        _logger.LogInformation("Cancel POST method called.");
        _logger.LogInformation($"User ID: {user.Id}");

        if (user.ActiveScienceId.HasValue)
        {
            _logger.LogInformation($"Active Science ID: {user.ActiveScienceId.Value}");
            var science = await _context.Sciences.FirstOrDefaultAsync(s => s.Id == user.ActiveScienceId);
            if (science != null)
            {
                _logger.LogInformation($"Cancelling research: {science.Name}");
                science.ResearchStartTime = null;
                science.ResearchEndTime = null;
                user.ActiveScienceId = null;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Research cancelled successfully.");
            }
            else
            {
                _logger.LogWarning("Science not found for the active research.");
            }
        }
        else
        {
            _logger.LogWarning("No active research to cancel.");
        }

        return RedirectToAction("Index");
    }



    private List<Science> GetScienceTypes()
    {
        return new List<Science>
        {
            new Science {
                Id = 1,
                Name = "Jet engine",
                IronCost = 100,
                SilverCost = 200,
                AluminiumCost = 150,
                H2Cost = 100,
                EnergyCost = 50,
                Duration = 60,
                Description = "The Jet Drive is an essential propulsion system that enables basic yet efficient travel between celestial bodies. It provides the fundamental means to navigate the vastness of space, ensuring reliable and consistent movement for all types of spacecraft within your empire."
            },
            new Science {
                Id = 2,
                Name = "Lineardrive",
                IronCost = 150,
                SilverCost = 250,
                AluminiumCost = 200,
                H2Cost = 120,
                EnergyCost = 70,
                Duration = 90,
                Description = "Increases drive efficiency."
            },
            new Science { Id = 3,
                Name = "Transitorialdrive",
                IronCost = 200,
                SilverCost = 300,
                AluminiumCost = 250,
                H2Cost = 140,
                EnergyCost = 90,
                Duration = 120,
                Description = "Enhances transitorial drive capabilities."
            },
            new Science { Id = 4,
                Name = "Laser beam concentrator",
                IronCost = 250,
                SilverCost = 350,
                AluminiumCost = 300,
                H2Cost = 160,
                EnergyCost = 110,
                Duration = 150,
                Description = "Concentrates laser beam power."
            },
            new Science {
                Id = 5,
                Name = "Parashield",
                IronCost = 300,
                SilverCost = 400,
                AluminiumCost = 350,
                H2Cost = 180,
                EnergyCost = 130,
                Duration = 180,
                Description = "Increases shield strength."
            },
            new Science { Id = 6,
                Name = "Impulse beam",
                IronCost = 350,
                SilverCost = 450,
                AluminiumCost = 400,
                H2Cost = 200,
                EnergyCost = 150,
                Duration = 210,
                Description = "Enhances impulse beam damage."
            },
            new Science {
                Id = 7,
                Name = "Iron Mining",
                IronCost = 400,
                SilverCost = 500,
                AluminiumCost = 450,
                H2Cost = 220,
                EnergyCost = 170,
                Duration = 240,
                Description = "Improves iron mining efficiency by 3% per level."
            }
        };
    }
}
