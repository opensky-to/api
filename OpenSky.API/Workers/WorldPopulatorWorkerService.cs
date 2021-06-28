// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulatorWorkerService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Worker that checks all airports for missing population (Flag set to false)
    /// Then invokes the WorldPopulator for that airport.
    /// </summary>
    /// <remarks>
    /// Flusinerd, 25/06/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Extensions.Hosting.BackgroundService"/>
    /// -------------------------------------------------------------------------------------------------
    public class WorldPopulatorWorkerService : BackgroundService
    {
        /// -------------------------------------------------------------------------------------------------        
        /// <summary>
        /// The clean up interval in milliseconds.
        /// 1 minute
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const int CheckInterval = 1 * 60 * 1000;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<WorldPopulatorWorkerService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The services.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IServiceProvider services;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The world populator service instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly WorldPopulatorService worldPopulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPopulatorWorkerService"/> class.
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
        /// <param name="worldPopulator">
        /// The world populator service instance.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulatorWorkerService(
            IServiceProvider services,
            ILogger<WorldPopulatorWorkerService> logger,
            WorldPopulatorService worldPopulator
        )
        {
            this.services = services;
            this.logger = logger;
            this.worldPopulator = worldPopulator;
        }

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
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("World populator background service stopping...");
            await base.StopAsync(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        /// <seealso cref="M:Microsoft.Extensions.Hosting.BackgroundService.ExecuteAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("World populator background service starting...");
            await this.CheckAirports(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Search for airports that need populating with aircraft and process them.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task CheckAirports(CancellationToken stoppingToken)
        {
            using var scope = this.services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OpenSkyDbContext>();

            // When starting up, reset airports that still have "Queued" status back to "NeedsHandling", in case the server stopped while processing an airport
            try
            {
                var leftOverQueuedAirports = db.Airports.Where(a => a.HasBeenPopulated == ProcessingStatus.Queued);
                foreach (var airport in leftOverQueuedAirports)
                {
                    airport.HasBeenPopulated = ProcessingStatus.NeedsHandling;
                }

                await db.SaveDatabaseChangesAsync(this.logger, "Error saving airport HasBeenPopulated resets.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error resetting leftover queued airports during world populator background worker startup.");
            }

            var todoCount = 0;
            int? processed = null;
            var runStarted = DateTime.UtcNow;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var airportTodoCount = await db.Airports.CountAsync(airport => airport.HasBeenPopulated == ProcessingStatus.NeedsHandling && airport.Size.HasValue && airport.MSFS, stoppingToken);
                    if (airportTodoCount > 0)
                    {
                        if (!processed.HasValue)
                        {
                            processed = 0;
                            todoCount = airportTodoCount;
                            runStarted = DateTime.UtcNow;
                            this.logger.LogInformation($"World populator starting new run, found {airportTodoCount} airports to process...");
                        }

                        // Get the first 100 airports that need populating
                        var airports = await db.Airports.Where(airport => airport.HasBeenPopulated == ProcessingStatus.NeedsHandling && airport.Size.HasValue && airport.MSFS).Take(100).ToListAsync(stoppingToken);
                        await this.worldPopulator.CheckAndGenerateAircaftForAirports(airports, stoppingToken);
                        processed += airports.Count;
                        this.logger.LogInformation($"World populator processed {processed} of {todoCount} airports [{(int)((processed / (double)todoCount) * 100)} %]");
                    }
                    else
                    {
                        if (processed.HasValue)
                        {
                            this.logger.LogInformation($"World populator processed {processed} airports in {(DateTime.UtcNow - runStarted).TotalMinutes:F1} minutes.");
                            todoCount = 0;
                            processed = null;
                        }

                        // Nothing to do right now, wait for CheckInterval
                        await Task.Delay(CheckInterval, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error processing airports to populate.");
                    await Task.Delay(30 * 1000, stoppingToken);
                }
            }
        }
    }
}