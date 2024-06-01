using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StellarIO.Models;
using StellarIO.Services;

namespace StellarIO.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class GalaxyController : ControllerBase
    {
        private readonly GalaxyService _galaxyService;
        private readonly ILogger<GalaxyController> _logger;

        public GalaxyController(GalaxyService galaxyService, ILogger<GalaxyController> logger)
        {
            _galaxyService = galaxyService;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Galaxy> Get() 
        {
            return _galaxyService.GetGalaxies();
        }

        [HttpGet("{galaxyId}")]
        public ActionResult<GalaxyViewModel> GetGalaxy(int galaxyId)
        {
            var galaxy = _galaxyService.GetGalaxiesFull().Where(g => g.Id == galaxyId).AsViewModel().FirstOrDefault();
            if (galaxy == null)
            {
                return NotFound($"Galaxy with Id {galaxyId} not found");
            }
            return galaxy;
        }
    }
}
