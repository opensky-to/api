// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AirportController : ControllerBase
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
        private readonly ILogger<AirportController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirportController(ILogger<AirportController> logger, OpenSkyDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all available airports (ICAO code only).
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <returns>
        /// All available airports.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet(Name = "GetAirports")]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetAirports()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airport");
            var icaos = await this.db.Airports.Select(a => a.ICAO).ToListAsync();
            return new ApiResponse<IEnumerable<string>>(icaos);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport");
                return new ApiResponse<IEnumerable<string>>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets one specific airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/05/2021.
        /// </remarks>
        /// <param name="icao">
        /// The ICAO identifier of the airport.
        /// </param>
        /// <returns>
        /// The airport record if found, nothing otherwise.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("{icao}", Name = "GetAirport")]
        public async Task<ActionResult<ApiResponse<Airport>>> GetAirport(string icao)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airport/{icao}");
            var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO.Equals(icao));
            return new ApiResponse<Airport>(airport);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport/{icao}");
                return new ApiResponse<Airport>(ex);
            }
        }
    }
}