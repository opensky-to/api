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
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Airport;
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
                var packageEntries = await this.db.Airports.Where(a => a.Size.HasValue).Select(a => new AirportClientPackageEntry(a)).ToListAsync();

                var package = new AirportClientPackageRoot(packageEntries);

                var jObject = JObject.FromObject(package);
                var jsonString = jObject.ToString(Formatting.None);

                using var sha256Hash = SHA256.Create();
                var jsonHash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
                var hashBase64 = Convert.ToBase64String(jsonHash);

                var latestClientPackage = await this.db.AirportClientPackages.OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();
                if (latestClientPackage == null || !string.Equals(latestClientPackage.PackageHash, hashBase64, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Create new client package
                    var newPackage = new AirportClientPackage
                    {
                        CreationTime = DateTime.Now,
                        PackageHash = hashBase64,
                        Package = jsonString.CompressToBase64()
                    };

                    await this.db.AirportClientPackages.AddAsync(newPackage);
                    var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new airport client package.");
                    if (saveEx != null)
                    {
                        return new ApiResponse<string>("Error saving new airport client package", saveEx);
                    }

                    return new ApiResponse<string>("Successfully created new client airport package.");
                }
                else
                {
                    return new ApiResponse<string>("Skipped, no changes since last airport client package.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Airport/clientPackage");
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
                var airport = await this.db.Airports.Where(a => a.HasBeenPopulated == status).Take(maxResults).ToListAsync();
                // todo add country(ies) by looking up ICAO registration
                return new ApiResponse<IEnumerable<Airport>>(airport);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AirportsWithPopulationStatus/{status}/{maxResults}");
                return new ApiResponse<IEnumerable<Airport>>(ex);
            }
        }
    }
}