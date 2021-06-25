// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulatorService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CsvHelper;
    using CsvHelper.Configuration;

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
        /// The icao registrations from the CSV file.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IcaoRegistration[] icaoRegistrations;

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
            //SEP, MEP, SET, MET, JET, Regional, NBAirliner, WBAirliner
            { 0, 0, 0, 0, 0, 0, 0, 0 }, // -1
            { 0.85, 0.15, 0, 0, 0, 0, 0, 0 }, // 0
            { 0.7, 0.15, 0.05, 0.05, 0.05, 0, 0, 0 }, // 1
            { 0.4, 0.2, 0.1, 0.1f, 0.1, 0.05, 0.05, 0 }, // 2
            { 0.15, 0.3, 0.15, 0.1, 0.1, 0.1, 0.1, 0 }, // 3
            { 0.075, 0.075, 0.1, 0.05, 0.1, 0.2, 0.3, 0.1 }, // 4
            { 0.05, 0.05, 0.05, 0.05, 0.05, 0.15, 0.35, 0.25 }, // 5
            { 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.3, 0.4, } // 6 
        };

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
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulatorService(IServiceProvider services, ILogger<WorldPopulatorService> logger)
        {
            this.logger = logger;
            this.db = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            var reader = new StreamReader("Datasets/ICAO.csv");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };

            var csv = new CsvReader(reader, config);
            this.icaoRegistrations = csv.GetRecords<IcaoRegistration>().ToArray();
            this.logger.LogInformation("Populator Service running");
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
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task CheckAndGenerateAircraftForAirport(Airport airport)
        {
            airport.HasBeenPopulated = ProcessingStatus.Queued;
            await this.db.SaveChangesAsync();
            var aircraftAtAirport = this.db.Aircraft.Where(aircraft => aircraft.AirportICAO == airport.ICAO);
            var availableForPurchaseOrRent = aircraftAtAirport.Where(aircraft => aircraft.RentPrice.HasValue || aircraft.PurchasePrice.HasValue);
            var totalRamps = airport.GaRamps;
            var totalGates = airport.Gates;
            double totalSlots = totalGates + totalRamps;

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

            // 80% Utilization
            var requiredAircraft = Math.Ceiling(totalSlots * 0.8);

            var newAircraftCount = availableForPurchaseOrRent.Count();
            int[] generatedTypesCount = { 0, 0, 0, 0, 0, 0, 0, 0 };

            // Check if less then 80% are available
            if (newAircraftCount < requiredAircraft)
            {
                // Check the currently available aircraft types distribution
                var seps = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.SEP);
                var meps = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.MEP);
                var sets = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.SET);
                var mets = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.MET);
                var jets = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.Jet);
                var regionals = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.Regional);
                var nbAirliners = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.NBAirliner);
                var wbAirliners = availableForPurchaseOrRent.Where(aircraft => aircraft.Type.Category == AircraftTypeCategory.WBAirliner);

                // Look up the target ratios for the current airport size (0-indexed, therefor Airport Size + 1)
                // @todo refactor to enum
                var sepTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.SEP];
                var mepTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.MEP];
                var setTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.SET];
                var metTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.MET];
                var jetTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.Jet];
                var regionalTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.Regional];
                var nbTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.NBAirliner];
                var wbTarget = this.ratios[airport.Size.GetValueOrDefault() + 1, (int)AircraftTypeCategory.WBAirliner];

                var generatedAircraft = new List<Aircraft>();

                var wasSuccessfull = true;

                while (newAircraftCount < requiredAircraft)
                {
                    var sepQuota = (await seps.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.SEP]) / requiredAircraft;
                    var mepQuota = (await meps.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.MEP]) / requiredAircraft;
                    var setQuota = (await sets.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.SET]) / requiredAircraft;
                    var metQuota = (await mets.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.MET]) / requiredAircraft;
                    var jetQuota = (await jets.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.Jet]) / requiredAircraft;
                    var regionalQuota = (await regionals.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.Regional]) / requiredAircraft;
                    var nbQuota = (await nbAirliners.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.NBAirliner]) / requiredAircraft;
                    var wbQuota = (await wbAirliners.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.WBAirliner]) / requiredAircraft;

                    // Determine the highest delta
                    var sepDelta = sepQuota - sepTarget;
                    var mepDelta = mepQuota - mepTarget;
                    var setDelta = setQuota - setTarget;
                    var metDelta = metQuota - metTarget;
                    var jetDelta = jetQuota - jetTarget;
                    var regionalDelta = regionalQuota - regionalTarget;
                    var nbDelta = nbQuota - nbTarget;
                    var wbDelta = wbQuota - wbTarget;

                    // IMPORTANT: HAS TO BE IN ORDER OF AircraftTypeCategory enum
                    double[] deltas = { sepDelta, mepDelta, setDelta, metDelta, jetDelta, regionalDelta, nbDelta, wbDelta };
                    var minIndex = FindMinInArray(deltas);

                    try
                    {
                        // Generate Registration for country
                        var registration = this.GenerateRegistration(airport);

                        // Get random enabled vanilla type of needed category
                        var typeCandidates = this.db.AircraftTypes.Where(type => type.Category == (AircraftTypeCategory)minIndex && type.Enabled && type.IsVanilla && type.MinimumRunwayLength <= airport.LongestRunwayLength);

                        var alternateIndex = minIndex;

                        while (!await typeCandidates.AnyAsync() && alternateIndex > 0)
                        {
                            // Go down a category, if no suitable plane could be found
                            alternateIndex = minIndex switch
                            {
                                (int)AircraftTypeCategory.MEP => (int)AircraftTypeCategory.SEP,
                                (int)AircraftTypeCategory.SET => (int)AircraftTypeCategory.MEP,
                                (int)AircraftTypeCategory.MET => (int)AircraftTypeCategory.SET,
                                (int)AircraftTypeCategory.Jet => (int)AircraftTypeCategory.MET,
                                (int)AircraftTypeCategory.Regional => (int)AircraftTypeCategory.Jet,
                                (int)AircraftTypeCategory.NBAirliner => (int)AircraftTypeCategory.Regional,
                                (int)AircraftTypeCategory.WBAirliner => (int)AircraftTypeCategory.NBAirliner,
                                _ => (int)AircraftTypeCategory.NBAirliner,
                            };

                            var localIndex = alternateIndex;
                            typeCandidates = this.db.AircraftTypes.Where(type => type.Category == (AircraftTypeCategory)localIndex && type.Enabled && type.IsVanilla && type.MinimumRunwayLength <= airport.LongestRunwayLength);
                        }

                        // todo replace this with random? technically works but seems to generate certain plane types over and over again and not distributing evenly
                        var randomType = await typeCandidates.OrderBy(x => Guid.NewGuid()).FirstAsync();

                        // @todo update when economics are implemented
                        var purchasePrice = (randomType.MaxPrice + randomType.MinPrice) / 2;
                        var rentPrice = purchasePrice / 100;

                        // Create aircraft with picked type
                        var aircraft = new Aircraft
                        {
                            AirportICAO = airport.ICAO,
                            PurchasePrice = purchasePrice,
                            RentPrice = rentPrice,
                            Registry = registration,
                            TypeID = randomType.ID
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
                this.db.Aircraft.AddRange(generatedAircraft);
                await this.db.SaveChangesAsync();
                this.logger.LogInformation($"Generated {newAircraftCount} aircraft for airport {airport.ICAO}");
            }
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
            return $"P{Random.Next(500, 999)}";
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

            // Length of prefix plus 1 to accomodate for the dash
            var prefixLength = prefix.Length;
            if (withDash)
            {
                prefixLength = prefix.Length + 1;
            }

            var randomString = RandomString(length - prefixLength);
            if (withDash)
            {
                return $"{prefix}-{randomString}";
            }

            return prefix + randomString;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a Taiwanese registration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/06/2021.
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
            var randomNum = Random.Next(0, 100);
            return randomNum switch
            {
                < 33 => "N" + Random.Next(1, 99999), // Generate 1-999999
                < 66 => "N" + Random.Next(1, 9999) + RandomString(1), // Generate 1A-9999Z
                _ => "N" + Random.Next(1, 999) + RandomString(2), // Generate 1AA-999ZZ
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
        /// <returns>
        /// Unique Registration as string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private string GenerateRegistration(Airport airport)
        {
            var airportRegistrationsEntry = this.icaoRegistrations.FirstOrDefault(icaoRegistration => airport.ICAO.ToLower()[..2].StartsWith(icaoRegistration.AirportPrefix.ToLower()));
            const int maxAttempts = 10;
            var registration = "";

            // Default to US Registration
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
                    "HJ" => GenerateColombiaRegistration(),
                    "CU" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "E3" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "3DC" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "JA" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6, false),
                    "UP" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 8),
                    "P" => GenerateNorthKoreaRegistration(),
                    "HL" => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6, false),
                    "RDPL" => GenerateLaosRegistration(),
                    "N" => GenerateUsRegistration(),
                    "B" => GenerateTaiwanRegistration(),
                    _ => GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6)
                };

                // Lookup DB for registration
                var registrationExists = this.db.Aircraft.Any(aircraft => aircraft.Registry == registration);
                if (!registrationExists)
                {
                    // Found a non existing registration
                    break;
                }

                if (i == 9)
                {
                    throw new Exception("Could not find a non-duplicate registration for aircraft");
                }
            }

            return registration;
        }
    }
}