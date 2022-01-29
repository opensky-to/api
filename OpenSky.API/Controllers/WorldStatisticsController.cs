// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldStatisticsController.cs" company="OpenSky">
// OpenSky project 2021-2022
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

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World statistics controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class WorldStatisticsController : ControllerBase
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
        private readonly ILogger<WorldStatisticsController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The world populator service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly AircraftPopulatorService aircraftPopulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The statistics service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly StatisticsService statisticsService;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldStatisticsController"/> class.
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
        /// <param name="aircraftPopulator">
        /// The world populator service.
        /// </param>
        /// <param name="statisticsService">
        /// The statistics service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldStatisticsController(ILogger<WorldStatisticsController> logger, OpenSkyDbContext db, AircraftPopulatorService aircraftPopulator, StatisticsService statisticsService)
        {
            this.logger = logger;
            this.db = db;
            this.aircraftPopulator = aircraftPopulator;
            this.statisticsService = statisticsService;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get world statistics overview.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the world statistics overview.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("", Name = "GetWorldStatisticsOverview")]
        public async Task<ActionResult<ApiResponse<WorldStatisticsOverview>>> GetWorldStatisticsOverview()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET WorldStatistics");

                var fuelAvailability = new List<PieChartValue>
                {
                    new() { Key = "None", Value = await this.db.Airports.CountAsync(a => !a.HasAvGas && !a.HasJetFuel) },
                    new() { Key = "AvGas", Value = await this.db.Airports.CountAsync(a => a.HasAvGas && !a.HasJetFuel) },
                    new() { Key = "Jetfuel", Value = await this.db.Airports.CountAsync(a => !a.HasAvGas && a.HasJetFuel) },
                    new() { Key = "Both", Value = await this.db.Airports.CountAsync(a => a.HasAvGas && a.HasJetFuel) }
                };

                var runwayLights = new List<PieChartValue>
                {
                    new() { Key = "Lit", Value = await this.db.Runways.CountAsync(r => !string.IsNullOrEmpty(r.CenterLight) || !string.IsNullOrEmpty(r.EdgeLight)) },
                    new() { Key = "Unlit", Value = await this.db.Runways.CountAsync(r => string.IsNullOrEmpty(r.CenterLight) && string.IsNullOrEmpty(r.EdgeLight)) }
                };

                var aircraftOwner = new List<PieChartValue>
                {
                    new() { Key = "System", Value = await this.db.Aircraft.CountAsync(a => a.OwnerID == null) },
                    new() { Key = "Player", Value = await this.db.Aircraft.CountAsync(a => a.OwnerID != null) },
                    new() { Key = "Airline", Value = await this.db.Aircraft.CountAsync(a => a.AirlineOwnerID != null) }
                };

                var overview = new WorldStatisticsOverview
                {
                    TotalAirports = await this.db.Airports.CountAsync(),
                    AirportSizes = await this.db.Airports.GroupBy(a => a.Size, a => a, (size, airports) => new PieChartValue { Key = $"{size}", Value = airports.Count() }).ToListAsync(),
                    FuelAvailability = fuelAvailability,
                    TotalRunways = await this.db.Runways.CountAsync(),
                    RunwaySurfaces = await this.db.Runways.GroupBy(r => r.Surface, r => r, (surface, runways) => new PieChartValue { Key = $"{surface.ParseRunwaySurface()}", Value = runways.Count() }).ToListAsync(),
                    RunwayLights = runwayLights,
                    TotalApproaches = await this.db.Approaches.CountAsync(),
                    ApproachTypes = await this.db.Approaches.GroupBy(a => a.Type, a => a, (type, approaches) => new PieChartValue { Key = type, Value = approaches.Count() }).ToListAsync(),
                    TotalAircraft = await this.db.Aircraft.CountAsync(),
                    AircraftCategories = await this.db.Aircraft.GroupBy(a => a.Type.Category, a => a, (category, aircraft) => new PieChartValue { Key = $"{category}", Value = aircraft.Count() }).ToListAsync(),
                    AircraftOwner = aircraftOwner,
                    TotalJobs = await this.statisticsService.GetTotalJobCount(),
                    JobsGenerated = await this.statisticsService.GetJobGeneratedCount(),
                    JobOperators = this.statisticsService.GetJobOperatorPieSeries(),
                    JobTypes = this.statisticsService.GetJobTypePieSeries(),
                    JobAircraftCategories = this.statisticsService.GetJobAircraftTypePieSeries(),
                    CompletedFlights = await this.statisticsService.GetTotalFlightCount(),
                    FlightOperators = this.statisticsService.GetFlightOperatorPieSeries(),
                    FlightAircraftCategories = this.statisticsService.GetFlightAircraftTypePieSeries()
                };

                return new ApiResponse<WorldStatisticsOverview>(overview);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET WorldStatistics");
                return new ApiResponse<WorldStatisticsOverview>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Manually request to populate an airport and return info text results.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/07/2021.
        /// </remarks>
        /// <param name="icao">
        /// The icao of the airport to populate.
        /// </param>
        /// <returns>
        /// An information string reporting what operations/errors where performed/encountered.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("populateAircraft/{icao}", Name = "PopulateAirportWithAircraft")]
        public async Task<ActionResult<ApiResponse<string>>> PopulateAirportWithAircraft(string icao)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST WorldStatistics/populateAircraft/{icao}");

                var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == icao);
                if (airport == null)
                {
                    return new ApiResponse<string>($"No airport record found for ICAO {icao}.") { IsError = true };
                }

                var infoText = await this.aircraftPopulator.CheckAndGenerateAircraftForAirport(airport, false);
                return new ApiResponse<string>($"Finished populating airport {icao}") { Data = infoText };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST WorldStatistics/populateAircraft/{icao}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}