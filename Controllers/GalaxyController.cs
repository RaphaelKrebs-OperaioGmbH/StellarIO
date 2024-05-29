using Microsoft.AspNetCore.Mvc;
using StellarIO.Models;
using StellarIO.Services;
using System.Linq;

public class GalaxyController : Controller
{
    private readonly GalaxyService _galaxyService;

    public GalaxyController(GalaxyService galaxyService)
    {
        _galaxyService = galaxyService;
    }

    public IActionResult Index()
    {
        var galaxy = _galaxyService.GetGalaxies().AsViewModel().FirstOrDefault();
        return View(galaxy);
    }
}
