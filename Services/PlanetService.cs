using Microsoft.AspNetCore.Identity;
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
    }
}
