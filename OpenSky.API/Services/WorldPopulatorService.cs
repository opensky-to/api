// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulatorService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.Datasets;
    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Service that is used for populating the world with aircraft.
    /// </summary>
    /// <remarks>
    /// Flusinerd, 25/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldPopulatorService
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
        /// The chances that an airport of a given size will spawn a random foreign registration instead
        /// of a local one - indexed by airport size, excluding -1 ;).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly int[] foreignRegChances = { 2, 5, 7, 10, 15, 20, 25 };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<WorldPopulatorService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ratios for the different airport sizes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly double[,] ratios =
        {
            //SEP, MEP, SET, MET, JET, Regional, NBAirliner, WBAirliner, Helicopter
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // -1
            { 0.8, 0.15, 0, 0, 0, 0, 0, 0, 0.05 }, // 0
            { 0.65, 0.15, 0.05, 0.05, 0.05, 0, 0, 0, 0.05 }, // 1
            { 0.35, 0.2, 0.1, 0.1f, 0.1, 0.05, 0.05, 0, 0.05 }, // 2
            { 0.15, 0.25, 0.15, 0.1, 0.1, 0.1, 0.1, 0, 0.05 }, // 3
            { 0.075, 0.075, 0.05, 0.05, 0.1, 0.2, 0.3, 0.1, 0.05 }, // 4
            { 0.05, 0.05, 0.05, 0.05, 0.05, 0.15, 0.35, 0.25, 0 }, // 5
            { 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.3, 0.4, 0 } // 6 
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ICAO registrations service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IcaoRegistrationsService icaoRegistrations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPopulatorService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 16/06/2021.
        /// </remarks>
        /// <param name="services">
        /// The services context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="icaoRegistrations">
        /// The ICAO registrations service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulatorService(IServiceProvider services, ILogger<WorldPopulatorService> logger, IcaoRegistrationsService icaoRegistrations)
        {
            this.logger = logger;
            this.db = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            this.icaoRegistrations = icaoRegistrations;
            this.logger.LogInformation("World populator service started");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks the airports for missing quotas and generates new aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/06/2021.
        /// </remarks>
        /// <param name="airports">
        /// The list of airports to bulk-process.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task CheckAndGenerateAircaftForAirports(IEnumerable<Airport> airports, CancellationToken cancellationToken)
        {
            // Comments on how this works have been removed for the bulk-operation method, to understand what this code does look at the single airport method
            var generatedAircraft = new List<Aircraft>();
            var updatedAirports = new List<Airport>();
            var aircraftTypes = await this.db.AircraftTypes.ToListAsync(cancellationToken);
            foreach (var airport in airports)
            {
                try
                {
                    if (!airport.Size.HasValue || !airport.MSFS)
                    {
                        continue;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var availableForPurchaseOrRent = await this.db.Aircraft.Where(aircraft => aircraft.AirportICAO == airport.ICAO && (aircraft.RentPrice.HasValue || aircraft.PurchasePrice.HasValue)).ToListAsync(cancellationToken);
                    var totalSlots = CalculateTotalSlots(airport);
                    var requiredAircraft = Math.Ceiling(totalSlots * 0.8);
                    var newAircraftCount = availableForPurchaseOrRent.Count;
                    if (newAircraftCount < requiredAircraft)
                    {
                        var categoryCount = Enum.GetValues<AircraftTypeCategory>().Length;
                        var generatedTypesCount = new int[categoryCount];
                        var counts = new int[categoryCount];
                        var targets = new double[categoryCount];
                        foreach (var category in Enum.GetValues<AircraftTypeCategory>())
                        {
                            counts[(int)category] = availableForPurchaseOrRent.Count(a => a.Type.Category == category);
                            targets[(int)category] = this.ratios[airport.Size.Value + 1, (int)category];
                        }

                        var wasSuccessfull = true;
                        while (newAircraftCount < requiredAircraft)
                        {
                            var quotas = new double[categoryCount];
                            var deltas = new double[categoryCount];
                            foreach (var category in Enum.GetValues<AircraftTypeCategory>())
                            {
                                quotas[(int)category] = (counts[(int)category] + generatedTypesCount[(int)category]) / requiredAircraft;
                                deltas[(int)category] = quotas[(int)category] - targets[(int)category];
                            }

                            var minIndex = FindMinInArray(deltas);
                            try
                            {
                                var registration = this.GenerateRegistration(airport, generatedAircraft);
                                var typeCandidates = aircraftTypes.Where(
                                    type => type.Category == (AircraftTypeCategory)minIndex && type.Enabled && (type.IsVanilla || type.IncludeInWorldPopulation) && type.MinimumRunwayLength <= airport.LongestRunwayLength).ToList();

                                var alternateIndex = minIndex;
                                while (typeCandidates.Count == 0 && alternateIndex > 0)
                                {
                                    alternateIndex--;
                                    var queryIndex = alternateIndex;
                                    typeCandidates = aircraftTypes.Where(
                                        type => type.Category == (AircraftTypeCategory)queryIndex && type.Enabled && (type.IsVanilla || type.IncludeInWorldPopulation) && type.MinimumRunwayLength <= airport.LongestRunwayLength).ToList();
                                }

                                var randomType = typeCandidates[Random.Next(0, typeCandidates.Count - 1)];
                                // todo @todo update when economics/aircraft age/wear/tear are implemented
                                var purchasePrice = (int)Math.Round((randomType.MaxPrice + randomType.MinPrice) / 2.0 * (Random.Next(80, 120) / 100.0), 0);
                                purchasePrice = Math.Max(Math.Min(randomType.MaxPrice, purchasePrice), randomType.MinPrice); // Make sure we stay within the min/max limit no matter what
                                var rentPrice = purchasePrice / 200;
                                var fuel = Math.Round(randomType.FuelTotalCapacity * (Random.Next(30, 100) / 100.0), 2);

                                var aircraft = new Aircraft
                                {
                                    AirportICAO = airport.ICAO,
                                    PurchasePrice = purchasePrice,
                                    RentPrice = rentPrice,
                                    Registry = registration,
                                    TypeID = randomType.ID,
                                    Fuel = fuel
                                };

                                generatedAircraft.Add(aircraft);
                                generatedTypesCount[minIndex]++;
                                newAircraftCount++;
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogError(ex, $"Error during aircraft generation for {airport.ICAO}");
                                wasSuccessfull = false;
                                break;
                            }
                        }

                        airport.HasBeenPopulated = wasSuccessfull ? ProcessingStatus.Finished : ProcessingStatus.Failed;
                        updatedAirports.Add(airport);
                    }
                    else
                    {
                        airport.HasBeenPopulated = ProcessingStatus.Finished;
                        updatedAirports.Add(airport);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Error populating airport {airport.ICAO} with aircraft.");
                    airport.HasBeenPopulated = ProcessingStatus.Failed;
                    updatedAirports.Add(airport);
                }
            }

            try
            {
                await this.db.BulkInsertAsync(generatedAircraft, cancellationToken);
                await this.db.BulkUpdateAsync(updatedAirports, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error bulk-saving generated aircraft and updated airports.");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks the airport for missing quotas and generates new aircraft.
        /// </summary>
        /// <remarks>
        /// Flusinerd+sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to process.
        /// </param>
        /// <param name="throwAllExceptions">
        /// (Optional) True to throw all exceptions (false to just log them and set error status on airport).
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> CheckAndGenerateAircraftForAirport(Airport airport, bool throwAllExceptions = true)
        {
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

            // The info text that describes what we did, for manual calls by admins
            string infoText;

            // ReSharper disable once AccessToModifiedClosure
            var availableForPurchaseOrRent = await this.db.Aircraft.Where(aircraft => aircraft.AirportICAO == airport.ICAO && (aircraft.RentPrice.HasValue || aircraft.PurchasePrice.HasValue)).ToListAsync();
            var aircraftTypes = await this.db.AircraftTypes.ToListAsync();
            var totalSlots = CalculateTotalSlots(airport);

            // 80% Utilization ?
            var requiredAircraft = Math.Ceiling(totalSlots * 0.8);
            var newAircraftCount = availableForPurchaseOrRent.Count;
            if (newAircraftCount < requiredAircraft)
            {
                infoText = $"Airport {airport.ICAO} has {newAircraftCount} aircraft available, but {requiredAircraft} are required, populating...\r\n";

                var categoryCount = Enum.GetValues<AircraftTypeCategory>().Length;
                var generatedTypesCount = new int[categoryCount];
                var counts = new int[categoryCount];
                var targets = new double[categoryCount];
                foreach (var category in Enum.GetValues<AircraftTypeCategory>())
                {
                    // Check the currently available aircraft types distribution
                    counts[(int)category] = availableForPurchaseOrRent.Count(a => a.Type.Category == category);

                    // Look up the target ratios for the current airport size (0-indexed, therefore Airport Size + 1)
                    targets[(int)category] = this.ratios[airport.Size.Value + 1, (int)category];
                }

                var generatedAircraft = new List<Aircraft>();
                var errors = new List<Exception>();
                while (newAircraftCount < requiredAircraft)
                {
                    var quotas = new double[categoryCount];
                    var deltas = new double[categoryCount];
                    foreach (var category in Enum.GetValues<AircraftTypeCategory>())
                    {
                        quotas[(int)category] = (counts[(int)category] + generatedTypesCount[(int)category]) / requiredAircraft;

                        // Determine the highest delta
                        // IMPORTANT: HAS TO BE IN ORDER OF AircraftTypeCategory enum
                        deltas[(int)category] = quotas[(int)category] - targets[(int)category];
                    }

                    var minIndex = FindMinInArray(deltas);
                    try
                    {
                        // Generate registration for the aircraft (based on airport country)
                        var registration = this.GenerateRegistration(airport, generatedAircraft);

                        // Get enabled vanilla/popular types of needed category and matching minimum runway length
                        var typeCandidates = aircraftTypes.Where(
                            type => type.Category == (AircraftTypeCategory)minIndex && type.Enabled && (type.IsVanilla || type.IncludeInWorldPopulation) && type.MinimumRunwayLength <= airport.LongestRunwayLength).ToList();

                        var alternateIndex = minIndex;
                        while (typeCandidates.Count == 0 && alternateIndex > 0)
                        {
                            // Go down a category, if no suitable aircraft type could be found
                            alternateIndex--;

                            var queryIndex = alternateIndex;
                            typeCandidates = aircraftTypes.Where(
                                type => type.Category == (AircraftTypeCategory)queryIndex && type.Enabled && (type.IsVanilla || type.IncludeInWorldPopulation) && type.MinimumRunwayLength <= airport.LongestRunwayLength).ToList();
                        }

                        // Pick a random type and set purchase and rent price
                        var randomType = typeCandidates[Random.Next(0, typeCandidates.Count - 1)];
                        // todo @todo update when economics/aircraft age/wear/tear are implemented
                        var purchasePrice = (int)Math.Round((randomType.MaxPrice + randomType.MinPrice) / 2.0 * (Random.Next(80, 120) / 100.0), 0);
                        purchasePrice = Math.Max(Math.Min(randomType.MaxPrice, purchasePrice), randomType.MinPrice); // Make sure we stay within the min/max limit no matter what
                        var rentPrice = purchasePrice / 200;
                        var fuel = Math.Round(randomType.FuelTotalCapacity * (Random.Next(30, 100) / 100.0), 2);

                        // Create aircraft with picked type
                        var aircraft = new Aircraft
                        {
                            AirportICAO = airport.ICAO,
                            PurchasePrice = purchasePrice,
                            RentPrice = rentPrice,
                            Registry = registration,
                            TypeID = randomType.ID,
                            Fuel = fuel
                        };

                        infoText += $"{airport.ICAO}: Generated new aircraft with registration {registration}, category {randomType.Category} and type {randomType.Name} ({randomType.ID})\r\n";

                        generatedAircraft.Add(aircraft);
                        generatedTypesCount[minIndex]++;
                        newAircraftCount++;
                    }
                    catch (Exception ex)
                    {
                        infoText += $"Error during aircraft generation for {airport.ICAO}\r\n{ex}";
                        this.logger.LogError(ex, $"Error during aircraft generation for {airport.ICAO}");
                        errors.Add(ex);
                        break;
                    }
                }

                if (!this.db.IsAttached(airport))
                {
                    airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == airport.ICAO);
                }

                airport.HasBeenPopulated = errors.Count == 0 ? ProcessingStatus.Finished : ProcessingStatus.Failed;
                await this.db.Aircraft.AddRangeAsync(generatedAircraft);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, $"Error saving generated aircraft for airport {airport.ICAO}.");
                if (saveEx == null)
                {
                    this.logger.LogDebug($"Generated {newAircraftCount} aircraft for airport {airport.ICAO}");
                    infoText += $"Generated {newAircraftCount} aircraft for airport {airport.ICAO}";

                    if (errors.Count > 0 && throwAllExceptions)
                    {
                        throw errors[0];
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(infoText))
                    {
                        infoText += "\r\n";
                    }

                    infoText += $"Error saving generated aircraft for airport {airport.ICAO}.\r\n{saveEx}";

                    if (throwAllExceptions)
                    {
                        throw saveEx;
                    }
                }
            }
            else
            {
                // Airport had enough aircraft already
                infoText = $"Airport {airport.ICAO} has enough aircraft already ({requiredAircraft} required, {newAircraftCount} available), skipping.";
                if (!this.db.IsAttached(airport))
                {
                    airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == airport.ICAO);
                }

                airport.HasBeenPopulated = ProcessingStatus.Finished;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, $"Error setting Finished status on airport {airport.ICAO}");
                if (saveEx != null)
                {
                    if (!string.IsNullOrEmpty(infoText))
                    {
                        infoText += "\r\n";
                    }

                    infoText += $"Error setting Finished status on airport {airport.ICAO}.\r\n{saveEx}";

                    if (throwAllExceptions)
                    {
                        throw saveEx;
                    }
                }
            }

            return infoText;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate total slots for the specified airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/06/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to process.
        /// </param>
        /// <returns>
        /// The calculated total slots.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static double CalculateTotalSlots(Airport airport)
        {
            var totalSlots = (double)(airport.Gates + airport.GaRamps);

            // If the airport is closed, don't generate any aircraft
            if (airport.Size == -1)
            {
                totalSlots = 0;
            }

            // Set total slots if 0 (>18k airports don't have this information set)
            if (totalSlots == 0)
            {
                totalSlots = airport.Size switch
                {
                    -1 => 0,
                    0 => 4.07,
                    1 => 6.42,
                    2 => 12.66,
                    3 => 21.94 + 0.35,
                    4 => 37.14 + 2.75,
                    5 => 71.45 + 22.16,
                    6 => 115.59 + 87.27,
                    _ => totalSlots
                };

                if (totalSlots > 0)
                {
                    var tenPercent = (int)Math.Round(totalSlots * 0.1);
                    totalSlots += Random.Next(-tenPercent, tenPercent);
                }
            }

            return totalSlots;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches for the index of the minimum element in the specified array or list.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <param name="list">
        /// The array/list.
        /// </param>
        /// <returns>
        /// The index of the minimum item.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static int FindMinInArray(IReadOnlyList<double> list)
        {
            var pos = 0;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] < list[pos])
                {
                    pos = i;
                }
            }

            return pos;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates an Armenian registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/06/2021.
        /// </remarks>
        /// <returns>
        /// The Armenian registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateArmeniaRegistration()
        {
            return $"EK-{Random.Next(0, 99999):D6}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Colombian registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <returns>
        /// The Colombian registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateColombiaRegistration()
        {
            return $"HK-{Random.Next(1000, 9999)}{RandomString(1)}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Laos registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <returns>
        /// The Laos registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateLaosRegistration()
        {
            return $"RDPL-{Random.Next(10000, 99999)}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a North Korean registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
        /// </remarks>
        /// <returns>
        /// The nNorth Korean registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateNorthKoreaRegistration()
        {
            return Random.Next(0, 100) switch
            {
                < 20 => $"P{Random.Next(500, 9999)}", // 20% chance to spawn a longer number, we need more numbers to populate North Korea correctly to 80%
                _ => $"P{Random.Next(500, 999)}"
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Pseudo-Random aircraft registration for the provided country, with the provided
        /// length.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 25/06/2021.
        /// </remarks>
        /// <param name="prefixes">
        /// String array of possible prefixes of the country, with element 0 being the mainly used one.
        /// </param>
        /// <param name="length">
        /// Length of the registration.
        /// </param>
        /// <param name="withDash">
        /// (Optional)
        /// If registration should contain a dash between the prefix and the random string.
        /// </param>
        /// <returns>
        /// Registration as string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateRegularRegistration(IReadOnlyList<string> prefixes, int length, bool withDash = true)
        {
            var prefix = prefixes[0];

            // Check if it has an alt prefix
            if (prefixes.Count > 1)
            {
                // 30% Chance for alt prefix
                if (Random.Next(0, 100) < 30)
                {
                    prefix = prefixes[Random.Next(0, prefixes.Count - 1)];
                }
            }

            // Some prefixes (especially British oversea territories) already have a dash followed by a letter
            if (prefix.Contains("-"))
            {
                withDash = false;
            }

            // Length of prefix plus 1 to accomodate for the dash
            var prefixLength = prefix.Length;
            if (withDash)
            {
                prefixLength = prefix.Length + 1;
            }

            var randomString = RandomString(length - prefixLength);
            return withDash ? $"{prefix}-{randomString}" : $"{prefix}{randomString}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Russian registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/06/2021.
        /// </remarks>
        /// <returns>
        /// The Russian registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateRussiaRegistration()
        {
            return $"RA-{Random.Next(0, 99999):D6}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a South Korean registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/06/2021.
        /// </remarks>
        /// <returns>
        /// The South Korean registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateSouthKoreaRegistration()
        {
            return $"HL{Random.Next(1000, 9699):D4}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Taiwanese registration.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 25/06/2021.
        /// </remarks>
        /// <returns>
        /// The Taiwanese registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateTaiwanRegistration()
        {
            return $"B-{Random.Next(0, 99999):D6}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a US registration.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 25/06/2021.
        /// </remarks>
        /// <returns>
        /// The US registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string GenerateUsRegistration()
        {
            return Random.Next(0, 100) switch
            {
                < 33 => $"N{Random.Next(1, 99999)}", // Generate 1-999999
                < 66 => $"N{Random.Next(1, 9999)}{RandomString(1)}", // Generate 1A-9999Z
                _ => $"N{Random.Next(1, 999)}{RandomString(2)}", // Generate 1AA-999ZZ
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a random upper case string with the provided length.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 25/06/2021.
        /// </remarks>
        /// <param name="length">
        /// Length of the generated string.
        /// </param>
        /// <returns>
        /// Pseudo-Random string of length.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a new registration for a country. Checks the database to avoid duplicates.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 16/06/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="airport">
        /// Airport this registration should be generated for.
        /// </param>
        /// <param name="generatedAircraft">
        /// The already generated aircraft to avoid duplicates.
        /// </param>
        /// <returns>
        /// Unique Registration as string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private string GenerateRegistration(Airport airport, IReadOnlyCollection<Aircraft> generatedAircraft)
        {
            var airportRegistrationsEntry = this.icaoRegistrations.GetIcaoRegistrationForAirport(airport);
            if (airportRegistrationsEntry == null)
            {
                throw new Exception($"Unable to find ICAO registration for airport {airport.ICAO}.");
            }

            const int maxAttempts = 20;
            var registration = string.Empty;

            // Pick a random foreign registration?
            var randomReg = Random.Next(1, 100);
            if (randomReg <= this.foreignRegChances[airport.Size ?? 0])
            {
                airportRegistrationsEntry = this.icaoRegistrations.GetRandomIcaoRegistration();
            }

            // Default to US Registration if there is no entry for a given country
            airportRegistrationsEntry ??= new IcaoRegistration
            {
                AircraftPrefix = "N",
                AirportPrefix = "K",
                Country = "United States of America"
            };

            for (var i = 0; i < maxAttempts; i++)
            {
                registration = airportRegistrationsEntry.AircraftPrefixes[0] switch
                {
                    "HK" => GenerateColombiaRegistration(),
                    "CU" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "E3" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "3DC" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "JA" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6, false),
                    "UP" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 8),
                    "P" => GenerateNorthKoreaRegistration(),
                    "HL" => GenerateSouthKoreaRegistration(),
                    "RDPL" => GenerateLaosRegistration(),
                    "N" => GenerateUsRegistration(),
                    "B" => GenerateTaiwanRegistration(),
                    "EK" => GenerateArmeniaRegistration(),
                    "RA" => GenerateRussiaRegistration(),
                    _ => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6)
                };

                // Lookup DB for registration
                var registrationExists = this.db.Aircraft.Any(aircraft => aircraft.Registry == registration) || generatedAircraft.Any(aircraft => aircraft.Registry == registration);
                if (!registrationExists)
                {
                    // Found a non existing registration
                    break;
                }

                if (i == maxAttempts - 1)
                {
                    // The US numbers have space for >1 million planes, pick a random US one if we can't find a local one
                    if (airportRegistrationsEntry.AircraftPrefix != "N")
                    {
                        i = 0;
                        airportRegistrationsEntry = new IcaoRegistration
                        {
                            AircraftPrefix = "N",
                            AirportPrefix = "K",
                            Country = "United States of America"
                        };
                    }
                    else
                    {
                        throw new Exception("Could not find a non-duplicate registration for aircraft");
                    }
                }
            }

            return registration;
        }
    }
}