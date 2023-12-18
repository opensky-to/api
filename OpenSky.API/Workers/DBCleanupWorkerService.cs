// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBCleanupWorkerService.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;

    using GeoCoordinatePortable;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.S2Geometry.Extensions;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// DB cleanup background worker service.
    /// </summary>
    /// <remarks>
    /// Flusinerd, 13/06/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Extensions.Hosting.BackgroundService"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class DbCleanupWorkerService : BackgroundService
    {
        /// -------------------------------------------------------------------------------------------------        
        /// <summary>
        /// The clean-up interval in milliseconds (30 minutes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const int CleanupInterval = 30 * 60 * 1000;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The error interval in milliseconds (1 minute).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const int ErrorInterval = 1 * 60 * 1000;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<DbCleanupWorkerService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The service provider.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IServiceProvider services;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="DbCleanupWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static DbCleanupWorkerService()
        {
            Status = new Dictionary<Guid, DataImportStatus>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DbCleanupWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DbCleanupWorkerService(
            IServiceProvider services,
            ILogger<DbCleanupWorkerService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the status dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Dictionary<Guid, DataImportStatus> Status { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Indicates that the shutdown process should no longer be graceful.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// <seealso cref="M:Microsoft.Extensions.Hosting.BackgroundService.StopAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("DB cleanup background service stopping...");
            return base.StopAsync(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long-
        /// running operation(s) being performed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/12/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long-running operations.
        /// </returns>
        /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService.ExecuteAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("DB cleanup background service starting...");
            using var scope = this.services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OpenSkyDbContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var errorCount = 0;
                errorCount += await this.CleanupOpenSkyTokens(db);
                errorCount += await this.CleanupAirportClientPackages(db);
                errorCount += await this.CleanupExpiredJobs(db);
                errorCount += await this.CancelStaleFlights(db);

                await Task.Delay(errorCount == 0 ? CleanupInterval : ErrorInterval, stoppingToken);
            }
        }

        private async Task<int> CancelStaleFlights(OpenSkyDbContext db)
        {
            try
            {
                var started7DaysAgo = DateTime.UtcNow.AddDays(-7);
                var flights = await db.Flights
                                      .Where(f => f.Started.HasValue && !f.Paused.HasValue && !f.Completed.HasValue && f.Started.Value < started7DaysAgo)
                                      .Include(flight => flight.Origin)
                                      .Include(flight => flight.Aircraft)
                                      .ThenInclude(aircraft => aircraft.Type)
                                      .ToListAsync();
                foreach (var flight in flights)
                {
                    // ==============================================================================
                    // CHANGES TO THIS CODE NEED TO BE MIRRORED IN FlightController.AbortFlight METHOD!!!
                    // ==============================================================================

                    // Check if/what penalties to apply
                    if (flight.OnGround)
                    {
                        // What's the closest airport to the current location?
                        if (flight.Latitude.HasValue && flight.Longitude.HasValue)
                        {
                            // Check where we are (closest airport)
                            var closestAirportICAO = await this.GetClosestAirport(db, new GeoCoordinate(flight.Latitude.Value, flight.Longitude.Value), flight.Aircraft.Type.Simulator);
                            if (string.IsNullOrEmpty(closestAirportICAO))
                            {
                                // Landed at no airport, back to origin!
                                flight.Aircraft.AirportICAO = flight.OriginICAO;
                                flight.LandedAtICAO = flight.OriginICAO;
                            }
                            else
                            {
                                flight.Aircraft.AirportICAO = closestAirportICAO;
                                flight.LandedAtICAO = closestAirportICAO;
                            }

                            if (flight.TimeWarpTimeSavedSeconds > 0)
                            {
                                flight.Aircraft.WarpingUntil = DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds);
                            }

                            flight.Started = null;
                            flight.PayloadLoadingComplete = null;
                            flight.FuelLoadingComplete = null;
                            flight.AutoSaveLog = null;
                            flight.LastAutoSave = null;

                            flight.FlightPhase = FlightPhase.Briefing;
                            flight.Latitude = null; // We can leave the rest of the properties as this is now invalid for resume
                            flight.Longitude = null;
                            flight.LastPositionReport = null;
                            flight.OnGround = true;
                            flight.Altitude = null;
                            flight.GroundSpeed = null;
                            flight.Heading = null;
                        }
                        else
                        {
                            // Flight never report a position, so never started moving, easiest abort, no punishment, simply revert back to plan
                            flight.Started = null;
                            flight.PayloadLoadingComplete = null;
                            flight.FuelLoadingComplete = null;
                            flight.AutoSaveLog = null;
                            flight.LastAutoSave = null;
                            flight.LastPositionReport = null;
                            flight.OnGround = true;
                            flight.Altitude = null;
                            flight.GroundSpeed = null;
                            flight.Heading = null;
                        }
                    }
                    else
                    {
                        // Now it should get expensive, return flight, etc.
                        // todo penalties (money cost, fuel consumption?, reputation impact)

                        var distanceToOrigin = 0d;
                        if (flight.Latitude.HasValue && flight.Longitude.HasValue)
                        {
                            // Convert meters to nautical miles, before dividing by knots
                            distanceToOrigin = flight.Origin.GeoCoordinate.GetDistanceTo(new GeoCoordinate(flight.Latitude.Value, flight.Longitude.Value)) / 1852d;
                        }

                        var groundSpeed = flight.Aircraft.Type.EngineType switch
                        {
                            EngineType.HeloBellTurbine => 150,
                            EngineType.Jet => 400,
                            EngineType.Piston => 100,
                            EngineType.Turboprop => 250,
                            EngineType.None => 0,
                            EngineType.Unsupported => 0,
                            _ => 0
                        };

                        // ReSharper disable once PossibleInvalidOperationException
                        var returnFlightDuration = (distanceToOrigin > 0 && groundSpeed > 0) ? TimeSpan.FromHours(distanceToOrigin / groundSpeed) : DateTime.UtcNow - flight.Started.Value;
                        flight.Aircraft.WarpingUntil = flight.TimeWarpTimeSavedSeconds > 0 ? DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds).Add(returnFlightDuration) : DateTime.UtcNow.Add(returnFlightDuration);

                        // Increase airframe and engine hours
                        // ReSharper disable once PossibleInvalidOperationException
                        var hours = (DateTime.UtcNow - flight.Started.Value).TotalHours + returnFlightDuration.TotalHours;
                        flight.Aircraft.AirframeHours += hours;
                        switch (flight.Aircraft.Type.EngineCount)
                        {
                            case 1:
                                flight.Aircraft.Engine1Hours += hours;
                                break;
                            case 2:
                                flight.Aircraft.Engine1Hours += hours;
                                flight.Aircraft.Engine2Hours += hours;
                                break;
                            case 3:
                                flight.Aircraft.Engine1Hours += hours;
                                flight.Aircraft.Engine2Hours += hours;
                                flight.Aircraft.Engine3Hours += hours;
                                break;
                            case 4:
                                flight.Aircraft.Engine1Hours += hours;
                                flight.Aircraft.Engine2Hours += hours;
                                flight.Aircraft.Engine3Hours += hours;
                                flight.Aircraft.Engine4Hours += hours;
                                break;
                        }

                        // Return aircraft back to Origin airport
                        flight.Started = null;
                        flight.PayloadLoadingComplete = null;
                        flight.FuelLoadingComplete = null;
                        flight.AutoSaveLog = null;
                        flight.LastAutoSave = null;

                        flight.FlightPhase = FlightPhase.Briefing;
                        flight.Latitude = null; // We can leave the rest of the properties as this is now invalid for resume
                        flight.Longitude = null;
                        flight.LastPositionReport = null;
                        flight.OnGround = true;
                        flight.Altitude = null;
                        flight.GroundSpeed = null;
                        flight.Heading = null;
                    }
                }

                var saveEx = await db.SaveDatabaseChangesAsync(this.logger, "Error cancelling stale flights.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error cancelling stale flights.");
                return 1;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clean up previous airport client packages, only the most recent is required.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/12/2021.
        /// </remarks>
        /// <param name="db">
        /// The database context.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int, containing the error count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> CleanupAirportClientPackages(OpenSkyDbContext db)
        {
            try
            {
                var mostRecentPackage = db.AirportClientPackages.OrderByDescending(p => p.CreationTime).FirstOrDefault();
                if (mostRecentPackage != null)
                {
                    var olderPackages = db.AirportClientPackages.Where(p => p.CreationTime < mostRecentPackage.CreationTime);
                    db.AirportClientPackages.RemoveRange(olderPackages);
                    var saveEx = await db.SaveDatabaseChangesAsync(this.logger, "Error removing old airport client packages.");
                    if (saveEx != null)
                    {
                        throw saveEx;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error cleaning up airport client packages.");
                return 1;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cleans up expired jobs that aren't accepted from the database.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/12/2021.
        /// </remarks>
        /// <param name="db">
        /// The database context.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int, containing the error count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> CleanupExpiredJobs(OpenSkyDbContext db)
        {
            try
            {
                var jobs = db.Jobs.Where(j => string.IsNullOrEmpty(j.OperatorAirlineID) && string.IsNullOrEmpty(j.OperatorID) && j.ExpiresAt < DateTime.UtcNow);
                db.Jobs.RemoveRange(jobs);
                var saveEx = await db.SaveDatabaseChangesAsync(this.logger, "Error removing expired jobs.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error removing expired jobs.");
                return 1;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cleans up expired OpenSkyTokens from the database.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 14/06/2021.
        /// </remarks>
        /// <param name="db">
        /// The database context.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int, containing the error count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> CleanupOpenSkyTokens(OpenSkyDbContext db)
        {
            try
            {
                var tokens = db.OpenSkyTokens.Where(token => DateTime.UtcNow > token.Expiry);
                db.OpenSkyTokens.RemoveRange(tokens);
                var saveEx = await db.SaveDatabaseChangesAsync(this.logger, "Error removing old OpenSky tokens.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error cleaning up OpenSky tokens.");
                return 1;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find the closest airport to the specified geo coordinate.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// <param name="db">
        /// The database context.
        /// </param>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the closest airport ICAO code, or NULL if there is no
        /// airport within 10 nm.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<string> GetClosestAirport(OpenSkyDbContext db, GeoCoordinate location, Simulator simulator)
        {
            // Check where we are (closest airport)
            var coverage = location.CircularCoverage(10);
            var cells = coverage.Cells.Select(c => c.Id).ToList();
            var airports = await db.Airports.Where($"@0.Contains(S2Cell{coverage.Level})", cells).ToListAsync();
            if (simulator == Simulator.MSFS)
            {
                airports = airports.Where(a => a.MSFS).ToList();
            }

            if (simulator == Simulator.XPlane11)
            {
                airports = airports.Where(a => a.XP11).ToList();
            }

            if (airports.Count == 0)
            {
                // Landed at no airport, back to origin?
                return null;
            }

            Airport closest = null;
            double closestDistance = 9999;
            foreach (var airport in airports)
            {
                var distance = airport.GeoCoordinate.GetDistanceTo(location);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = airport;
                }
            }

            if (closest != null)
            {
                return closest.ICAO;
            }

            // Should not happen, take first airport from list I guess?
            return airports[0].ICAO;
        }
    }
}