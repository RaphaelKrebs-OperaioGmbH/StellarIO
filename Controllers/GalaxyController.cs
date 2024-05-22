using Microsoft.AspNetCore.Mvc;
using StellarIO.Models;
using System.Linq;

public class GalaxyController : Controller
{
    private readonly ApplicationDbContext _context;

    public GalaxyController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var galaxy = _context.Galaxies
            .Select(g => new GalaxyViewModel
            {
                Name = g.Name,
                Systems = g.Systems.Select(s => new GalaxySystemViewModel
                {
                    Id = s.Id,
                    Planets = s.Planets.Select(p => new PlanetViewModel
                    {
                        Name = p.Name,
                        Owner = p.User != null ? p.User.UserName : "Unowned",
                        Coordinates = $"{g.Id}:{s.Id}:{p.Id}"
                    }).ToList()
                }).ToList()
            }).FirstOrDefault();

        return View(galaxy);
    }
}
