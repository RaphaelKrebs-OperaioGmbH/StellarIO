using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StellarIO.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ApplicationDbContext context,
        ILogger<HomeController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Loading");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                _logger.LogInformation("Password verification result: {0}", result);
                var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToAction("Loading");
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt. Result: {0}", signInResult.ToString());
                }
            }
            else
            {
                _logger.LogWarning("User not found.");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Loading()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found");
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index");
            }

            var planets = await _context.Planets
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Buildings)
                .Include(p => p.System)
                .ThenInclude(s => s.Galaxy) // Ensure the related Galaxy is included
                .ToListAsync();

            return View(planets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading the dashboard.");
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPlanetStatus(int planetId)
    {
        var planet = await _context.Planets
            .Include(p => p.Buildings)
            .FirstOrDefaultAsync(p => p.Id == planetId);

        if (planet == null)
        {
            return NotFound();
        }

        var buildingInProgress = planet.Buildings.FirstOrDefault(b => b.ConstructionEndTime > DateTime.UtcNow);

        var result = new
        {
            building = buildingInProgress != null ? new
            {
                Name = buildingInProgress.Name,
                ConstructionEndTime = buildingInProgress.ConstructionEndTime?.ToString("o", CultureInfo.InvariantCulture),
                Id = buildingInProgress.Id
            } : null,
            resources = new
            {
                planet.Iron,
                planet.Silver,
                planet.Aluminium,
                planet.H2,
                planet.Energy
            }
        };

        return Json(result);
    }
}
