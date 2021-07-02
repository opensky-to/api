// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulationController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World population controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class WorldPopulationController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<WorldPopulationController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPopulationController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/05/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulationController(ILogger<WorldPopulationController> logger, OpenSkyDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get world population overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the world population overview.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("", Name = "GetWorldPopulationOverview")]
        public async Task<ActionResult<ApiResponse<WorldPopulationOverview>>> GetWorldPopulationOverview()
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET WorldPopulationOverview");
            var overview = new WorldPopulationOverview
            {
                TotalAirports = await this.db.Airports.CountAsync(),
                AirportSizes = await this.db.Airports.GroupBy(a => a.Size, a => a, (key, airports) => new AirportSize { Size = $"{key}", Airports = airports.Count() }).ToListAsync()
            };

            return new ApiResponse<WorldPopulationOverview>(overview);
        }
    }
}