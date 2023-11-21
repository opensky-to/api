// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportController.cs" company="OpenSky">
// OpenSky project 2021-2023
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
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Model.Extensions.AirportsJSON;

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
        /// Creates a new airport client package.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// The creation result string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("clientPackage", Name = "CreateAirportClientPackage")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> CreateAirportClientPackage()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Airport/clientPackage");
                var runways = await this.db.Runways.ToListAsync();
                var runwayEnds = await this.db.RunwayEnds.ToListAsync();
                var airports = await this.db.Airports.Where(a => a.Size.HasValue).ToListAsync();

                var airportEntries = new Dictionary<string, AirportsJSON.Airport>();
                foreach (var airport in airports)
                {
                    airportEntries.Add(airport.ICAO, airport.ConstructAirportsJson());
                }

                var runwayEntries = new Dictionary<int, AirportsJSON.Runway>();
                foreach (var runway in runways)
                {
                    runwayEntries.Add(runway.ID, runway.ConstructAirportsJson());
                    airportEntries[runway.AirportICAO].Runways.Add(runwayEntries[runway.ID]);
                }

                foreach (var runwayEnd in runwayEnds)
                {
                    runwayEntries[runwayEnd.RunwayID].RunwayEnds.Add(runwayEnd.ConstructAirportsJson());
                }

                var package = new AirportsJSON.AirportPackage();
                package.Airports.AddRange(airportEntries.Values);

                var compressedJson = package.CreatePackage();

                var latestClientPackage = await this.db.AirportClientPackages.OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();
                if (latestClientPackage == null || !string.Equals(latestClientPackage.PackageHash, package.Hash, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Create new client package
                    var newPackage = new AirportClientPackage
                    {
                        CreationTime = DateTime.UtcNow,
                        PackageHash = package.Hash,
                        Package = compressedJson
                    };

                    await this.db.AirportClientPackages.AddAsync(newPackage);
                    var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new airport client package.");
                    if (saveEx != null)
                    {
                        return new ApiResponse<string>("Error saving new airport client package", saveEx);
                    }

                    return new ApiResponse<string>("Successfully created new client airport package.");
                }

                return new ApiResponse<string>("Skipped, no changes since last airport client package.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Airport/clientPackage");
                return new ApiResponse<string>(ex);
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

                if (airport == null)
                {
                    return new ApiResponse<Airport>(Airport.ValidEmptyModel) { Message = $"No airport found for ICAO code {icao}" };
                }

                // todo add country(ies) by looking up ICAO registration
                return new ApiResponse<Airport>(airport);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport/{icao}");
                return new ApiResponse<Airport>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest airport client package.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// The airport client package.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("clientPackage", Name = "GetAirportClientPackage")]
        public async Task<ActionResult<ApiResponse<AirportClientPackage>>> GetAirportClientPackage()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airport/clientPackage");
                var latestClientPackage = await this.db.AirportClientPackages.OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();
                if (latestClientPackage == null)
                {
                    return new ApiResponse<AirportClientPackage>("No airport client package available.") { IsError = true };
                }

                return new ApiResponse<AirportClientPackage>(latestClientPackage);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport/clientPackage");
                return new ApiResponse<AirportClientPackage>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest airport client package hash.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <returns>
        /// The airport package hash.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("clientPackageHash", Name = "GetAirportClientPackageHash")]
        public async Task<ActionResult<ApiResponse<string>>> GetAirportClientPackageHash()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airport/clientPackageHash");
                var latestClientPackage = await this.db.AirportClientPackages.OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();
                if (latestClientPackage == null)
                {
                    return new ApiResponse<string>("No airport client package available.") { IsError = true };
                }

                return new ApiResponse<string> { Data = latestClientPackage.PackageHash };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport/clientPackageHash");
                return new ApiResponse<string>(ex);
            }
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
        /// Gets airports with the specified population status up to the specified max result count.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/07/2021.
        /// </remarks>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="maxResults">
        /// (Optional) The maximum results (default 50).
        /// </param>
        /// <returns>
        /// The matching airports.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("{status}/{maxResults:int}", Name = "GetAirportsWithPopulationStatus")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Airport>>>> GetAirportsWithPopulationStatus(ProcessingStatus status, int maxResults = 50)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AirportsWithPopulationStatus/{status}/{maxResults}");

                var airport = await this.db.Airports.Where(a => (a.HasBeenPopulatedMSFS == status && a.MSFS) || (a.HasBeenPopulatedXP11 == status && a.XP11)).OrderBy(a => a.ICAO).Take(maxResults).ToListAsync();

                // todo add country(ies) by looking up ICAO registration
                return new ApiResponse<IEnumerable<Airport>>(airport);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AirportsWithPopulationStatus/{status}/{maxResults}");
                return new ApiResponse<IEnumerable<Airport>>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches for airports using ICAO, name or city and return the first 50 results.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="searchString">
        /// The search string.
        /// </param>
        /// <returns>
        /// The matching airport records.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("search/{searchString}", Name = "SearchAirport")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Airport>>>> SearchAirport(string searchString)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airport/search/{searchString}");
                var airports = await this.db.Airports.Where(a => a.ICAO.Contains(searchString) || a.City.Contains(searchString) || a.Name.Contains(searchString)).Take(50).ToListAsync();
                return new ApiResponse<IEnumerable<Airport>>(airports);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Airport/search/{searchString}");
                return new ApiResponse<IEnumerable<Airport>>(ex);
            }
        }
    }
}