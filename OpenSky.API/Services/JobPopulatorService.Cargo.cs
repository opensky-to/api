// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobPopulatorService.Cargo.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model.Job;
    using OpenSky.API.Services.Models;
    using OpenSky.S2Geometry.Extensions;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Job populator service - cargo jobs.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class JobPopulatorService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cargo job configurations per category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<AircraftTypeCategory, CargoJobCategory> cargoJobConfig = new()
        {
            {
                AircraftTypeCategory.SEP,
                new CargoJobCategory
                {
                    Payload = 600,
                    MinDistance = 5,
                    MaxDistance = 150,
                    SkybucksPerNauticalMile = 0.3,
                    MinAirportSize = 1,
                    MaxAirportSize = 3,
                    MinHoursToExpiry = 2,
                    MaxHoursToExpiry = 48
                }
            },
            {
                AircraftTypeCategory.MEP,
                new CargoJobCategory
                {
                    Payload = 1100,
                    MinDistance = 30,
                    MaxDistance = 600,
                    SkybucksPerNauticalMile = 0.3,
                    MinAirportSize = 1,
                    MaxAirportSize = 3,
                    MinHoursToExpiry = 3,
                    MaxHoursToExpiry = 48
                }
            },
            {
                AircraftTypeCategory.SET,
                new CargoJobCategory
                {
                    Payload = 2500,
                    MinDistance = 50,
                    MaxDistance = 800,
                    SkybucksPerNauticalMile = 0.4,
                    MinAirportSize = 1,
                    MaxAirportSize = 3,
                    MinHoursToExpiry = 3,
                    MaxHoursToExpiry = 72
                }
            },
            {
                AircraftTypeCategory.MET,
                new CargoJobCategory
                {
                    Payload = 4500,
                    MinDistance = 50,
                    MaxDistance = 1100,
                    SkybucksPerNauticalMile = 0.4,
                    MinAirportSize = 2,
                    MaxAirportSize = 4,
                    MinHoursToExpiry = 4,
                    MaxHoursToExpiry = 72
                }
            },
            {
                AircraftTypeCategory.JET,
                new CargoJobCategory
                {
                    Payload = 6000,
                    MinDistance = 150,
                    MaxDistance = 1800,
                    SkybucksPerNauticalMile = 0.5,
                    MinAirportSize = 2,
                    MaxAirportSize = 5,
                    MinHoursToExpiry = 5,
                    MaxHoursToExpiry = 72
                }
            },
            {
                AircraftTypeCategory.REG,
                new CargoJobCategory
                {
                    Payload = 16000,
                    MinDistance = 150,
                    MaxDistance = 1200,
                    SkybucksPerNauticalMile = 0.5,
                    MinAirportSize = 3,
                    MaxAirportSize = 6,
                    MinHoursToExpiry = 5,
                    MaxHoursToExpiry = 96
                }
            },
            {
                AircraftTypeCategory.NBA,
                new CargoJobCategory
                {
                    Payload = 42000,
                    MinDistance = 150,
                    MaxDistance = 3000,
                    SkybucksPerNauticalMile = 0.5,
                    MinAirportSize = 3,
                    MaxAirportSize = 6,
                    MinHoursToExpiry = 5,
                    MaxHoursToExpiry = 96
                }
            },
            {
                AircraftTypeCategory.WBA,
                new CargoJobCategory
                {
                    Payload = 150000,
                    MinDistance = 500,
                    MaxDistance = 7000,
                    SkybucksPerNauticalMile = 0.5,
                    MinAirportSize = 4,
                    MaxAirportSize = 6,
                    MinHoursToExpiry = 10,
                    MaxHoursToExpiry = 96
                }
            },
            {
                AircraftTypeCategory.HEL,
                new CargoJobCategory
                {
                    Payload = 2000,
                    MinDistance = 5,
                    MaxDistance = 300,
                    SkybucksPerNauticalMile = 0.4,
                    MinAirportSize = 1,
                    MaxAirportSize = 4,
                    MinHoursToExpiry = 2,
                    MaxHoursToExpiry = 48
                }
            }
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of types of the cargoes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly string[] cargoTypes;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The minimum amount of cargo jobs that should be available per category and airport size.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly int[,] minCargoJobs =
        {
            // SEP, MEP, SET, MET, JET, REG, NBA, WBA, HEL
            {    0,   0,   0,   0,   0,   0,   0,   0,   0 }, // -1
            {   10,  10,   0,   0,   0,   0,   0,   0,   0 }, // 0
            {   10,  10,  10,   0,   0,   0,   0,   0,   5 }, // 1
            {   10,  10,  10,  10,  10,   0,   0,   0,   5 }, // 2
            {   10,  10,  10,  10,  10,  10,  10,   0,   5 }, // 3
            {   10,  10,  10,  10,  10,  10,  10,  10,   5 }, // 4
            {    0,   0,  10,  10,  10,  10,  10,  10,   0 }, // 5
            {    0,   0,   0,   0,  10,  10,  10,  10,   0 }  // 6
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks for missing quotas of cargo jobs at the specified airport and generates new ones to
        /// "refill" them.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to process.
        /// </param>
        /// <param name="availableJobs">
        /// The currently available jobs.
        /// </param>
        /// <param name="direction">
        /// The direction to generate jobs for.
        /// </param>
        /// <param name="targetCategory">
        /// (Optional) The aircraft type category to generate jobs for, set to NULL for all categories.
        /// </param>
        /// <returns>
        /// An asynchronous result returning an information string about what jobs were generated.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateCargoJobsForAirport(Airport airport, List<Job> availableJobs, JobDirection direction, AircraftTypeCategory? targetCategory = null)
        {
            if (direction == JobDirection.RoundTrip)
            {
                return "Skipping cargo jobs, not supported for round trips.";
            }

            var infoText = $"Processing cargo jobs for airport {airport.ICAO}, direction [{direction}]:\r\n";

            // Check job quota for each aircraft category depending on airport size
            foreach (var category in Enum.GetValues<AircraftTypeCategory>())
            {
                if (targetCategory.HasValue && targetCategory.Value != category)
                {
                    // We are only generating for a specific category, and it's not this one, skip over it
                    continue;
                }

                var config = this.cargoJobConfig[category];

                var coverage = airport.GeoCoordinate.DoughnutCoverage(config.MaxDistance, config.MinDistance);
                var includeCells = coverage.IncludeCells.Select(c => c.Id).ToList();
                var excludeCells = coverage.ExcludeCells.Select(c => c.Id).ToList();
                var destinations = await this.db.Airports.Where(
                    $"Size >= {config.MinAirportSize} and Size <= {config.MaxAirportSize} and !IsClosed and !IsMilitary and @0.Contains(S2Cell{coverage.IncludeLevel}) and not(@1.Contains(S2Cell{coverage.ExcludeLevel}))",
                    includeCells,
                    excludeCells).Select(a => new { a.ICAO, a.GeoCoordinate }).ToListAsync();

                if (destinations.Count == 0)
                {
                    infoText += $" - Unable find valid destination airports for cargo job category {category}\r\n";
                }
                else
                {
                    var generatedJobs = 0;
                    while (availableJobs.Count(j => j.Type == JobType.Cargo && j.Category == category) + generatedJobs < this.minCargoJobs[(airport.Size ?? -1) + 1, (int)category])
                    {
                        var destination = destinations[Random.Next(destinations.Count)];
                        var distanceToOrigin = airport.GeoCoordinate.GetDistanceTo(destination.GeoCoordinate) / 1852;
                        var tries = 10;
                        while (tries > 0 && (distanceToOrigin < config.MinDistance || distanceToOrigin > config.MaxDistance))
                        {
                            tries--;
                            destination = destinations[Random.Next(destinations.Count)];
                            distanceToOrigin = airport.GeoCoordinate.GetDistanceTo(destination.GeoCoordinate) / 1852;
                        }

                        if (distanceToOrigin < config.MinDistance || distanceToOrigin > config.MaxDistance)
                        {
                            infoText += $" - Unable find valid range destination airport after 10 tries for cargo job category {category}\r\n";
                            break;
                        }

                        var payloadPounds = Random.Next((int)(config.Payload * 0.3), config.Payload + 1);
                        var job = new Job
                        {
                            ID = Guid.NewGuid(),
                            OriginICAO = direction == JobDirection.From ? airport.ICAO : destination.ICAO,
                            ExpiresAt = DateTime.Now.AddHours(Random.Next(config.MinHoursToExpiry, config.MaxHoursToExpiry + 1)).AddMinutes(Random.Next(0, 60)),
                            Type = JobType.Cargo,
                            Category = category,
                            Value = (int)(payloadPounds * distanceToOrigin * config.SkybucksPerNauticalMile),
                        };

                        var payloads = new List<Payload>();
                        var payloadCount = Random.Next(1, 4);
                        var payloadDistribution = new[] { 0, 0, 0 };
                        var distributionLeft = 100;
                        for (var i = 0; i < payloadCount; i++)
                        {
                            if (i == payloadCount - 1)
                            {
                                // Last one, use the remaining distribution
                                payloadDistribution[i] = distributionLeft;
                            }
                            else
                            {
                                // Pick a new random double
                                var randomDistribution = Random.Next(10, Math.Min(distributionLeft, 70));
                                payloadDistribution[i] = randomDistribution;
                                distributionLeft -= randomDistribution;
                            }

                            payloads.Add(
                                new Payload
                                {
                                    JobID = job.ID,
                                    ID = Guid.NewGuid(),
                                    AirportICAO = direction == JobDirection.From ? airport.ICAO : destination.ICAO,
                                    DestinationICAO = direction == JobDirection.From ? destination.ICAO : airport.ICAO,
                                    Weight = Math.Round(payloadPounds * (payloadDistribution[i] / 100.0), 1),
                                    Description = this.cargoTypes[Random.Next(this.cargoTypes.Length)]
                                });
                        }

                        this.db.Jobs.Add(job);
                        await this.db.Payloads.AddRangeAsync(payloads);
                        infoText += $" - Created new cargo job[{category}] to {(direction == JobDirection.From ? destination.ICAO : airport.ICAO)} [{distanceToOrigin:F1} NM] worth $B {job.Value}, expiring {job.ExpiresAt}\r\n";
                        foreach (var payload in payloads)
                        {
                            infoText += $"   - Payload [{payload.Weight:F1} lbs]: {payload.Description}\r\n";
                        }

                        generatedJobs++;
                    }
                }
            }

            var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new jobs and payloads.");
            if (saveEx != null)
            {
                infoText += $" - Error saving new jobs and payloads: {saveEx.Message}";
            }

            return infoText;
        }
    }
}