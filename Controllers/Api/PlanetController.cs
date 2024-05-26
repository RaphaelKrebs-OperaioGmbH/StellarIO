﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StellarIO.Models;
using StellarIO.Services;

namespace StellarIO.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanetController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly PlanetService _planetService;
        private readonly ILogger<PlanetController> _logger;
        public PlanetController(PlanetService planetService, UserManager<User> userManager, ILogger<PlanetController> logger)
        {
            _planetService = planetService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Returns all planets as list
        /// NOTE: Very taxing on DB and server! Proper paging should be implemented here to avoid memory overflow
        /// </summary>
        /// <returns>List of all planets</returns>
        [HttpGet]
        public async Task<IEnumerable<Planet>> Get()
        {
            return await _planetService.GetAllPlanets().ToListAsync();
        }

        /// <summary>
        /// Returns all planets owned by the user calling the endpoint
        /// NOTE: Can be taxing on DB and server! Proper paging might be required.
        /// </summary>
        /// <returns>List of all planets owned by current user</returns>
        /// <exception cref="Exception">Current user claim not found in user manager service</exception>
        [HttpGet("mine")]
        public async Task<IEnumerable<Planet>> GetMyPlanets()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null)
            {
                _logger.LogError("User not found while retrieving Planets owned by user");
                throw new Exception("User not found while retrieving Planets owned by user");
            }
            return await _planetService.GetPlanetsOwnedByUser(me.Id).ToListAsync();
        }

        /// <summary>
        /// Gets a planet from DB by its Id, usefull for dashboard and stuff...
        /// NOTE: Any planet can be queried here - this will need some sort of anonymization for sure.
        /// E.g. if the current user does not own the planet, do not show all its properties
        /// </summary>
        /// <param name="planetId">Id of the planet requested</param>
        /// <returns>Planet with matching Id, or 404 not found status</returns>
        [HttpGet("{planetId}")]
        public async Task<ActionResult<Planet>> GetPlanet(int planetId)
        {
            var planet = await _planetService.GetPlanetAsync(planetId);
            if (planet == null)
            {
                return NotFound();
            }
            return planet;
        }
    }
}
