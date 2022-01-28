﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulationController.cs" company="OpenSky">
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
        /// The world populator service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly AircraftPopulatorService aircraftPopulator;

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
        /// <param name="aircraftPopulator">
        /// The world populator service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulationController(ILogger<WorldPopulationController> logger, OpenSkyDbContext db, AircraftPopulatorService aircraftPopulator)
        {
            this.logger = logger;
            this.db = db;
            this.aircraftPopulator = aircraftPopulator;
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
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET WorldPopulation");

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

                var flightOperators = new List<PieChartValue>
                {
                    new() { Key = "Player", Value = await this.db.Flights.CountAsync(f => f.Completed.HasValue && f.OperatorID != null)},
                    new() { Key = "Airline", Value = await this.db.Flights.CountAsync(f => f.Completed.HasValue && f.OperatorAirlineID != null)}
                };

                var overview = new WorldPopulationOverview
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
                    TotalJobs = await this.db.Jobs.CountAsync(),
                    JobTypes = await this.db.Jobs.GroupBy(j => j.Type, j => j, (type, jobs) => new PieChartValue { Key = $"{type}", Value = jobs.Count() }).ToListAsync(),
                    JobCategories = await this.db.Jobs.GroupBy(j => j.Category, j => j, (category, jobs) => new PieChartValue { Key = $"{category}", Value = jobs.Count() }).ToListAsync(),
                    CompletedFlights = await this.db.Flights.CountAsync(f => f.Completed.HasValue),
                    FlightOperators = flightOperators,
                    FlightAircraftCategories = this.db.Flights.Where(f => f.Completed.HasValue).ToList().GroupBy(f => f.Aircraft.Type.Category, f => f, (category, flights) => new PieChartValue { Key = $"{category}", Value = flights.Count() })
                };

                return new ApiResponse<WorldPopulationOverview>(overview);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET WorldPopulation");
                return new ApiResponse<WorldPopulationOverview>(ex);
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
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST WorldPopulation/populateAircraft/{icao}");

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
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST WorldPopulation/populateAircraft/{icao}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}