// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportController.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
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
        /// Gets all available airports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <returns>
        /// All available airports.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IEnumerable<Airport>> Get()
        {
            this.logger.LogInformation("Somebody requested the list of ALL airports (test log message)");
            return await this.db.Airports.ToListAsync();
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
        [HttpGet("{icao}", Name = "FindOne")]
        public async Task<ActionResult<Airport>> Get(string icao)
        {
            return await this.db.Airports.FirstOrDefaultAsync(a => a.ICAO.Equals(icao));
        }
    }
}