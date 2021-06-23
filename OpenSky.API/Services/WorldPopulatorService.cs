// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulatorService.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
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

    /// <summary>
    /// Service that is used for populating the world with aircraft
    /// </summary>
    public class WorldPopulatorService
    {
        private readonly OpenSkyDbContext dbContext;

        private readonly IcaoRegistration[] icaoRegistrations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<WorldPopulatorService> logger;

        /// <summary>
        /// Ratios for the different airport sizes
        /// </summary>
        private readonly double[,] ratios =
        {
            //SEP, MEP, SET, MET, JET, NBAirliner, WBAirliner, Regional
            { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, // -1
            { 0.85f, 0.15f, 0f, 0f, 0f, 0f, 0f, 0f }, // 0
            { 0.7f, 0.15f, 0.05f, 0.05f, 0.05f, 0f, 0f, 0f }, // 1
            { 0.4f, 0.2f, 0.1f, 0.1f, 0.1f, 0.05f, 0f, 0.05f }, // 2
            { 0.15f, 0.3f, 0.15f, 0.1f, 0.1f, 0.1f, 0f, 0.1f }, // 3
            { 0.075f, 0.075f, 0.1f, 0.05f, 0.1f, 0.3f, 0.1f, 0.2f }, // 4
            { 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.35f, 0.25f, 0.15f }, // 5
            { 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.3f, 0.4f, 0.05f } // 6 
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPopulatorService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 16/06/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="services">
        /// The services context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldPopulatorService(IServiceProvider services, ILogger<WorldPopulatorService> logger)
        {
            this.logger = logger;
            this.dbContext = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            var reader = new StreamReader("Datasets/ICAO.csv");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };
            var csv = new CsvReader(reader, config);
            this.icaoRegistrations = csv.GetRecords<IcaoRegistration>().ToArray();
            this.logger.LogInformation("Populator Service running");
        }

        /// <summary>
        /// Checks the airport for missing quotas and generates new aircraft
        /// </summary>
        /// <param name="airport"></param>
        public Task CheckAndGenerateAircraftForAirport(Airport airport)
        {
            return Task.Run(
                async () =>
                {
                    airport.HasBeenPopulated = Statuses.Queued;
                    await this.dbContext.SaveChangesAsync();
                    var aircraftAtAirport = this.dbContext.Aircraft.Where(aircraft => aircraft.AirportICAO == airport.ICAO);
                    var availableForPurchaseOrRent = aircraftAtAirport.Where(aircraft => aircraft.RentPrice.HasValue || aircraft.PurchasePrice.HasValue);
                    var totalRamps = airport.GaRamps;
                    var totalGates = airport.Gates;
                    var totalSlots = totalGates + totalRamps;

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
                            var nbQuota = (await nbAirliners.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.NBAirliner]) / requiredAircraft;
                            var wbQuota = (await wbAirliners.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.WBAirliner]) / requiredAircraft;
                            var regionalQuota = (await regionals.CountAsync() + generatedTypesCount[(int)AircraftTypeCategory.Regional]) / requiredAircraft;

                            // Determine the highest delta
                            var sepDelta = sepQuota - sepTarget;
                            var mepDelta = mepQuota - mepTarget;
                            var setDelta = setQuota - setTarget;
                            var metDelta = metQuota - metTarget;
                            var jetDelta = jetQuota - jetTarget;
                            var nbDelta = nbQuota - nbTarget;
                            var wbDelta = wbQuota - wbTarget;
                            var regionalDelta = regionalQuota - regionalTarget;

                            // IMPORTANT: HAS TO BE IN ORDER OF AircraftTypeCategory enum
                            double[] deltas = { sepDelta, mepDelta, setDelta, metDelta, jetDelta, nbDelta, wbDelta, regionalDelta };
                            var minIndex = FindMinInArray(deltas);

                            try
                            {
                                // Generate Registration for country
                                var registration = this.GenerateRegistration(airport);

                                // Get random enabled vanilla type of needed category
                                var typeCandidates = this.dbContext.AircraftTypes.Where(type => type.Category == (AircraftTypeCategory)minIndex && type.Enabled && type.IsVanilla && type.MinimumRunwayLength <= airport.LongestRunwayLength);

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
                                    typeCandidates = this.dbContext.AircraftTypes.Where(type => type.Category == (AircraftTypeCategory)alternateIndex && type.Enabled && type.IsVanilla && type.MinimumRunwayLength <= airport.LongestRunwayLength);
                                }

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
                            catch (Exception e)
                            {
                                this.logger.LogError("Error during aircraft generation for " + airport.ICAO + "\n" + e);
                                wasSuccessfull = false;
                                break;
                            }
                        }

                        airport.HasBeenPopulated = wasSuccessfull ? Statuses.Finished : Statuses.Failed;
                        this.dbContext.Aircraft.AddRange(generatedAircraft);
                        await this.dbContext.SaveChangesAsync();
                        this.logger.LogInformation("Generated " + newAircraftCount + " aircraft for airport " + airport.ICAO);
                    }
                });
        }

        private static int FindMinInArray(IReadOnlyList<double> arr)
        {
            var pos = 0;
            for (var i = 0; i < arr.Count; i++)
            {
                if (arr[i] < arr[pos])
                {
                    pos = i;
                }
            }

            return pos;
        }

        private static string GenerateLaosRegistration()
        {
            var random = new Random();
            return "" +
                   "RDPL" +
                   "-" + random.Next(10000, 99999);
        }

        private static string GenerateNorthKoreaRegistration()
        {
            var random = new Random();
            return "P" + random.Next(500, 999);
        }

        private static string GenerateTaiwanRegistration()
        {
            var random = new Random();
            return "B-" + random.Next(0, 99999).ToString("D6");
        }

        private string GenerateColombiaRegistration()
        {
            var random = new Random();
            return "HK-" + random.Next(1000, 9999) + this.RandomString(1);
        }

        /// <summary>
        /// Generates a new registration for a country. Checks the database to avoid duplicates.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 16/06/2021.
        /// </remarks>
        /// <param name="airport">
        /// Airport this registration should be generated for.
        /// </param>
        /// <returns>
        /// Unique Registration as string.
        /// </returns>
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
                    "HJ" => this.GenerateColombiaRegistration(),
                    "CU" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "E3" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "3DC" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 7),
                    "JA" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6, false),
                    "UP" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 8),
                    "P" => GenerateNorthKoreaRegistration(),
                    "HL" => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6, false),
                    "RDPL" => GenerateLaosRegistration(),
                    "N" => this.GenerateUsRegistration(),
                    "B" => GenerateTaiwanRegistration(),
                    _ => this.GenerateRegularRegistration(airportRegistrationsEntry.AircraftPrefixes, 6)
                };

                // Lookup DB for registration
                var registrationExists = this.dbContext.Aircraft.Any(aircraft => aircraft.Registry == registration);
                if (!registrationExists)
                {
                    // Found a non existing registration
                    break;
                }

                if (i == 9)
                {
                    throw new Exception("Could not find a non duplicate registration for aircraft");
                }
            }

            return registration;
        }

        /// <summary>
        /// Generates a Pseudo-Random aircraft registration for the provided country, with the provided length
        /// </summary>
        /// <param name="prefixes">
        /// String array of possible prefixes of the country, with element 0 being the mainly used one.
        /// </param>
        /// <param name="length">
        /// Length of the registration.
        /// </param>
        /// <param name="withDash">
        /// If registration should contain a dash between the prefix and the random string.
        /// </param>
        /// <returns>
        /// Registration as string
        /// </returns>
        private string GenerateRegularRegistration(IReadOnlyList<string> prefixes, int length, bool withDash = true)
        {
            var prefix = prefixes[0];

            var random = new Random();

            // Check if it has an alt prefix
            if (prefixes.Count > 1)
            {
                // 30% Chance for alt prefix
                if (random.Next(0, 100) < 30)
                {
                    prefix = prefixes[random.Next(0, prefixes.Count - 1)];
                }
            }

            // Length of prefix plus 1 to accomodate for the dash
            var prefixLength = prefix.Length;
            if (withDash)
            {
                prefixLength = prefix.Length + 1;
            }

            var randomString = this.RandomString(length - prefixLength);
            if (withDash)
            {
                return prefix + "-" + randomString;
            }

            return prefix + randomString;
        }

        private string GenerateUsRegistration()
        {
            var random = new Random();
            var randomNum = random.Next(0, 100);
            return randomNum switch
            {
                < 33 => "N" + random.Next(1, 99999), // Generate 1-999999
                < 66 => "N" + random.Next(1, 9999) + this.RandomString(1), // Generate 1A-9999Z
                _ => "N" + random.Next(1, 999) + this.RandomString(2), // Generate 1AA-999ZZ
            };
        }

        /// <summary>
        /// Generates a random upper case string with the provided length
        /// </summary>
        /// <param name="length">
        /// Length of the generated string
        /// </param>
        /// <returns>
        /// Pseudo-Random string of length
        /// </returns>
        private string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}