// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobPopulatorService.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model.Job;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Service that populates airports with jobs.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class JobPopulatorService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The random number generator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly Random Random = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The statistics service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly StatisticsService statisticsService;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<JobPopulatorService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="JobPopulatorService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="services">
        /// The services context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="statisticsService">
        /// The statistics service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public JobPopulatorService(IServiceProvider services, ILogger<JobPopulatorService> logger, StatisticsService statisticsService)
        {
            this.logger = logger;
            this.statisticsService = statisticsService;
            this.db = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            this.logger.LogInformation("Job populator service started");

            try
            {
                this.cargoTypes = File.ReadAllLines("Datasets/cargo_types.txt");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error loading cargo_types.txt file for job populator service!");
                this.cargoTypes = new[] { "Cargo" };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks the airport for missing quotas and generates new jobs as well as removing expired ones.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="icao">
        /// The ICAO code of the airport to process.
        /// </param>
        /// <param name="direction">
        /// The direction of jobs to generate.
        /// </param>
        /// <param name="category">
        /// (Optional) The aircraft type category to generate jobs for, set to NULL for all categories.
        /// </param>
        /// <param name="simulator">
        /// (Optional) The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result returning an information string about what jobs were generated.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateJobsForAirport(string icao, JobDirection direction, AircraftTypeCategory? category = null, Simulator? simulator = null)
        {
            // Check if there is an airport with that ICAO code
            var airport = this.db.Airports.SingleOrDefault(a => a.ICAO == icao);
            if (airport == null)
            {
                return $"No airport with ICAO code {icao}, not processing.";
            }

            if (simulator == Simulator.MSFS && !airport.MSFS)
            {
                return $"Airport {icao} is not available in MSFS, not processing.";
            }

            if (simulator == Simulator.XPlane11 && !airport.XP11)
            {
                return $"Airport {icao} is not available in XPlane11, not processing.";
            }

            // The airport doesn't have a size yet, so don't populate it
            if (!airport.Size.HasValue)
            {
                return $"Airport {airport.ICAO} has no size, not processing.";
            }

            // The airport is currently being imported (no sim), so don't populate it
            if (!airport.MSFS)
            {
                return $"Airport {airport.ICAO} has no active simulator, not processing.";
            }

            var started = DateTime.Now;
            var infoText = $"Checking and generating jobs for airport {icao}, direction [{direction}]:\r\n";

            // Remove expired jobs
            var expiredJobs = airport.Jobs.Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt <= DateTime.Now);
            this.db.Jobs.RemoveRange(expiredJobs);
            var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, $"Error removing expired jobs from airport {icao}.");
            if (saveEx != null)
            {
                infoText += $"Error removing expired jobs from airport {icao}: {saveEx.Message}\r\n";
            }

            // How many jobs are currently still available at this airport?
            var availableJobs = new List<Job>();
            if (direction == JobDirection.From)
            {
                availableJobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now).ToListAsync();
            }

            if (direction == JobDirection.To)
            {
                availableJobs = await this.db.Jobs.Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao)).ToListAsync();
            }

            if (direction == JobDirection.RoundTrip)
            {
                availableJobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao)).ToListAsync();
            }

            if (simulator == Simulator.MSFS)
            {
                availableJobs = availableJobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
            }

            if (simulator == Simulator.XPlane11)
            {
                availableJobs = availableJobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
            }

            if (category.HasValue)
            {
                availableJobs = availableJobs.Where(j => j.Category == category).ToList();
            }

            // Create the different job types
            infoText += await this.CheckAndGenerateCargoJobsForAirport(airport, availableJobs, direction, category, simulator);

            infoText += $"Finished processing job creation for airport {icao}, direction [{direction}] after {(DateTime.Now - started).TotalSeconds:F2} seconds.";
            return infoText;
        }
    }
}