using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using StellarIO.Models;

namespace StellarIO.Services
{
    public static partial class Extensions
    {
        public static IEnumerable<GalaxyViewModel> AsViewModel(this IEnumerable<Galaxy> galaxies)
        {
            return galaxies.Select(g => new GalaxyViewModel
            {
                Name = g.Name,
                Systems = g.Systems.Select(s => new GalaxySystemViewModel
                {
                    Id = s.Id,
                    Planets = s.Planets.Select(p => new PlanetViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Owner = p.User != null ? p.User.UserName ?? "Unnamed User" : "Unowned",
                        Coordinates = $"{g.Id}:{s.Id}:{p.Id}"
                    })
                })
            });
        }
    }
    public class GalaxyService
    {
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GalaxyService> _logger;
        public GalaxyService(ApplicationDbContext context, ILogger<GalaxyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual IEnumerable<Galaxy> GetGalaxies()
        {
            return _context.Galaxies
                .Include(g => g.Systems)
                .ThenInclude(s => s.Planets);
        }

    }
}
