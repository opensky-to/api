namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Services.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Service that populates airports with jobs.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class JobPopulatorService
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
        /// -------------------------------------------------------------------------------------------------
        public JobPopulatorService(IServiceProvider services, ILogger<JobPopulatorService> logger)
        {
            this.logger = logger;
            this.db = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            this.logger.LogInformation("Job populator service started");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks the airport for missing quotas and generates new jobs as well as removing expired ones.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to process.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateJobsForAirport(string icao)
        {
            // Check if there is an airport with that ICAO code
            var airport = this.db.Airports.SingleOrDefault(a => a.ICAO == icao);
            if (airport == null)
            {
                return $"No airport with ICAO code {icao}, not processing.";
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

            // Remove expired jobs
            var expiredJobs = airport.Jobs.Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt <= DateTime.Now);
            this.db.Jobs.RemoveRange(expiredJobs);

            // How many jobs are currently still available at this airport?
            var availableJobs = airport.Jobs.Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now).ToList();

            // Create the different job types
            var resultString = string.Empty;
            resultString += await this.CheckAndGenerateCargoJobsForAirport(airport, availableJobs);

            return string.Empty;
        }

        public async Task<string> CheckAndGenerateCargoJobsForAirport(Airport airport, List<Job> availableJobs)
        {
            var resultString = string.Empty;

            // Check job quota for each aircraft category depending on airport size
            foreach (var category in Enum.GetValues<AircraftTypeCategory>())
            {
                while (availableJobs.Count(j => j.Type == JobType.Cargo && j.Category == category) < this.minCargoJobs[(airport.Size ?? -1) + 1, (int)category])
                {

                }
            }

            return resultString;
        }

        private readonly int[,] minCargoJobs =
        {
            // SEP, MEP, SET, MET, JET, Regional, NBAirliner, WBAirliner, Helicopter
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // -1
            { 10, 0, 0, 0, 0, 0, 0, 0, 0 }, // 1
            { 10, 0, 0, 0, 0, 0, 0, 0, 0 }, // 2
            { 10, 0, 0, 0, 0, 0, 0, 0, 0 }, // 3
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 4
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 5
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 } // 6
        };

        private readonly Dictionary<AircraftTypeCategory, CargoJobCategory> cargoJobValues = new()
        {
            {
                AircraftTypeCategory.SEP,
                new CargoJobCategory
                {
                    Payload = 600,
                    MinDistance = 5,
                    MaxDistance = 150,
                    PaymentPerNM = 0.3
                }
            }
        };

    }
}
