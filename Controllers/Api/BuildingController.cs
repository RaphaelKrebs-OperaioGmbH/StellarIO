using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StellarIO.Models;
using StellarIO.Services;

namespace StellarIO.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class BuildingController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly BuildService _buildService;
        private readonly ILogger<PlanetController> _logger;
        public BuildingController(BuildService buildService, UserManager<User> userManager, ILogger<PlanetController> logger)
        {
            _buildService = buildService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("{buildingId}/cancel")]
        public async Task<ActionResult> CancelConstruction(int buildingId)
        {
            try
            {
                await _buildService.CancelBuildingAsync(buildingId);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e);
            }
        }
    }
}
