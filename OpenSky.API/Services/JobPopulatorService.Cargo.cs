// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobPopulatorService.Cargo.cs" company="OpenSky">
// OpenSky project 2021-2023
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
        /// Cargo job configurations per category (long).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<AircraftTypeCategory, CargoJobCategory> cargoLongJobConfig = new()
        {
            {
                AircraftTypeCategory.SEP,
                new CargoJobCategory
                {
                    Payload = 600,
                    MinDistance = 30,
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
                    MinDistance = 100,
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
                    MinDistance = 150,
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
                    MinDistance = 250,
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
                    MinDistance = 400,
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
                    MaxDistance = 400,
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
                    MinDistance = 500,
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
                    MinDistance = 1500,
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
                    MinDistance = 50,
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
        /// Cargo job configurations per category (short).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Dictionary<AircraftTypeCategory, CargoJobCategory> cargoShortJobConfig = new()
        {
            {
                AircraftTypeCategory.SEP,
                new CargoJobCategory
                {
                    Payload = 600,
                    MinDistance = 5,
                    MaxDistance = 30,
                    SkybucksPerNauticalMile = 0.25,
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
                    MaxDistance = 100,
                    SkybucksPerNauticalMile = 0.25,
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
                    MaxDistance = 150,
                    SkybucksPerNauticalMile = 0.35,
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
                    MaxDistance = 250,
                    SkybucksPerNauticalMile = 0.35,
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
                    MaxDistance = 400,
                    SkybucksPerNauticalMile = 0.45,
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
                    MaxDistance = 400,
                    SkybucksPerNauticalMile = 0.45,
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
                    MaxDistance = 500,
                    SkybucksPerNauticalMile = 0.45,
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
                    MaxDistance = 1500,
                    SkybucksPerNauticalMile = 0.45,
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
                    MaxDistance = 50,
                    SkybucksPerNauticalMile = 0.35,
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
        /// Checks for missing quotas of long cargo jobs at the specified airport and generates new ones to
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
        /// <param name="simulator">
        /// (Optional) The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result returning an information string about what jobs were generated.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateLongCargoJobsForAirport(Airport airport, List<Job> availableJobs, JobDirection direction, AircraftTypeCategory? targetCategory = null, Simulator? simulator = null)
        {
            if (direction == JobDirection.RoundTrip)
            {
                return "Skipping cargo jobs, not supported for round trips.";
            }

            var infoText = $"Processing long cargo jobs for airport {airport.ICAO}, direction [{direction}]:\r\n";

            // Check job quota for each aircraft category depending on airport size
            foreach (var category in Enum.GetValues<AircraftTypeCategory>())
            {
                if (targetCategory.HasValue && targetCategory.Value != category)
                {
                    // We are only generating for a specific category, and it's not this one, skip over it
                    continue;
                }

                var config = this.cargoLongJobConfig[category];

                var coverage = airport.GeoCoordinate.DoughnutCoverage(config.MaxDistance, config.MinDistance);
                var includeCells = coverage.IncludeCells.Select(c => c.Id).ToList();
                var excludeCells = coverage.ExcludeCells.Select(c => c.Id).ToList();

                var simClause = simulator switch
                {
                    Simulator.MSFS => " and MSFS ",
                    Simulator.XPlane11 => " and XP11 ",
                    _ => string.Empty
                };

                var destinations = await this.db.Airports.Where(
                    $"Size >= {config.MinAirportSize} and Size <= {config.MaxAirportSize} and !IsClosed and !IsMilitary{simClause} and @0.Contains(S2Cell{coverage.IncludeLevel}) and not(@1.Contains(S2Cell{coverage.ExcludeLevel}))",
                    includeCells,
                    excludeCells).Select(a => new { a.ICAO, a.GeoCoordinate }).ToListAsync();

                if (destinations.Count == 0)
                {
                    infoText += $" - Unable find valid destination airports for cargo_l job category {category}\r\n";
                }
                else
                {
                    var generatedJobs = 0;
                    while (availableJobs.Count(j => j.Type == JobType.Cargo_L && j.Category == category) + generatedJobs < this.minCargoJobs[(airport.Size ?? -1) + 1, (int)category])
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
                            infoText += $" - Unable find valid range destination airport after 10 tries for cargo_l job category {category}\r\n";
                            break;
                        }

                        var payloadPounds = Random.Next((int)(config.Payload * 0.3), config.Payload + 1);
                        var job = new Job
                        {
                            ID = Guid.NewGuid(),
                            OriginICAO = direction == JobDirection.From ? airport.ICAO : destination.ICAO,
                            ExpiresAt = DateTime.Now.AddHours(Random.Next(config.MinHoursToExpiry, config.MaxHoursToExpiry + 1)).AddMinutes(Random.Next(0, 60)),
                            Type = JobType.Cargo_L,
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
                        infoText += $" - Created new long cargo job[{category}] to {(direction == JobDirection.From ? destination.ICAO : airport.ICAO)} [{distanceToOrigin:F1} NM] worth $B {job.Value}, expiring {job.ExpiresAt}\r\n";
                        foreach (var payload in payloads)
                        {
                            infoText += $"   - Payload [{payload.Weight:F1} lbs]: {payload.Description}\r\n";
                        }

                        generatedJobs++;
                    }

                    this.statisticsService.RecordJobsGenerated(generatedJobs);
                }
            }

            var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new jobs and payloads.");
            if (saveEx != null)
            {
                infoText += $" - Error saving new jobs and payloads: {saveEx.Message}";
            }

            return infoText;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks for missing quotas of short cargo jobs at the specified airport and generates new ones to
        /// "refill" them.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/02/2022.
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
        /// <param name="simulator">
        /// (Optional) The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result returning an information string about what jobs were generated.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateShortCargoJobsForAirport(Airport airport, List<Job> availableJobs, JobDirection direction, AircraftTypeCategory? targetCategory = null, Simulator? simulator = null)
        {
            if (direction == JobDirection.RoundTrip)
            {
                return "Skipping cargo jobs, not supported for round trips.";
            }

            var infoText = $"Processing short cargo jobs for airport {airport.ICAO}, direction [{direction}]:\r\n";

            // Check job quota for each aircraft category depending on airport size
            foreach (var category in Enum.GetValues<AircraftTypeCategory>())
            {
                if (targetCategory.HasValue && targetCategory.Value != category)
                {
                    // We are only generating for a specific category, and it's not this one, skip over it
                    continue;
                }

                var config = this.cargoShortJobConfig[category];

                var coverage = airport.GeoCoordinate.DoughnutCoverage(config.MaxDistance, config.MinDistance);
                var includeCells = coverage.IncludeCells.Select(c => c.Id).ToList();
                var excludeCells = coverage.ExcludeCells.Select(c => c.Id).ToList();

                var simClause = simulator switch
                {
                    Simulator.MSFS => " and MSFS ",
                    Simulator.XPlane11 => " and XP11 ",
                    _ => string.Empty
                };

                var destinations = await this.db.Airports.Where(
                    $"Size >= {config.MinAirportSize} and Size <= {config.MaxAirportSize} and !IsClosed and !IsMilitary{simClause} and @0.Contains(S2Cell{coverage.IncludeLevel}) and not(@1.Contains(S2Cell{coverage.ExcludeLevel}))",
                    includeCells,
                    excludeCells).Select(a => new { a.ICAO, a.GeoCoordinate }).ToListAsync();

                if (destinations.Count == 0)
                {
                    infoText += $" - Unable find valid destination airports for cargo_s job category {category}\r\n";
                }
                else
                {
                    var generatedJobs = 0;
                    while (availableJobs.Count(j => j.Type == JobType.Cargo_S && j.Category == category) + generatedJobs < this.minCargoJobs[(airport.Size ?? -1) + 1, (int)category])
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
                            infoText += $" - Unable find valid range destination airport after 10 tries for cargo_s job category {category}\r\n";
                            break;
                        }

                        var payloadPounds = Random.Next((int)(config.Payload * 0.3), config.Payload + 1);
                        var job = new Job
                        {
                            ID = Guid.NewGuid(),
                            OriginICAO = direction == JobDirection.From ? airport.ICAO : destination.ICAO,
                            ExpiresAt = DateTime.Now.AddHours(Random.Next(config.MinHoursToExpiry, config.MaxHoursToExpiry + 1)).AddMinutes(Random.Next(0, 60)),
                            Type = JobType.Cargo_S,
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
                        infoText += $" - Created new short cargo job[{category}] to {(direction == JobDirection.From ? destination.ICAO : airport.ICAO)} [{distanceToOrigin:F1} NM] worth $B {job.Value}, expiring {job.ExpiresAt}\r\n";
                        foreach (var payload in payloads)
                        {
                            infoText += $"   - Payload [{payload.Weight:F1} lbs]: {payload.Description}\r\n";
                        }

                        generatedJobs++;
                    }

                    this.statisticsService.RecordJobsGenerated(generatedJobs);
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