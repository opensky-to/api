// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IcaoRegistrationsService.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using CsvHelper;
    using CsvHelper.Configuration;

    using Microsoft.Extensions.Logging;

    using OpenSky.API.Datasets;
    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// ICAO registrations for planes and countries service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class IcaoRegistrationsService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The random number generator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static readonly Random Random = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The icao registrations from the CSV file.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IcaoRegistration[] icaoRegistrations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="IcaoRegistrationsService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/07/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public IcaoRegistrationsService(ILogger<IcaoRegistrationsService> logger)
        {
            try
            {
                var reader = new StreamReader("Datasets/ICAO.csv");
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null
                };

                var csv = new CsvReader(reader, config);
                this.icaoRegistrations = csv.GetRecords<IcaoRegistration>().OrderByDescending(i => i.AirportPrefix).ToArray();
                csv.Dispose();
                reader.Close();
                reader.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading ICAO registrations from CSV file \"Datasets/ICAO.csv\".");
            }

            logger.LogInformation("ICAO registration service started");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ICAO registration record for the specified airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/07/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport.
        /// </param>
        /// <returns>
        /// The icao registration record for airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IcaoRegistration GetIcaoRegistrationForAirport(Airport airport)
        {
            return this.icaoRegistrations.FirstOrDefault(icao => airport.ICAO.ToLower()[..2].StartsWith(icao.AirportPrefix.ToLower()));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ICAO registration record(s) for the specified country.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/07/2021.
        /// </remarks>
        /// <param name="country">
        /// The country.
        /// </param>
        /// <returns>
        /// The ICAO registration record(s) for the specified country.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<IcaoRegistration> GetIcaoRegistrationsForCountry(Country country)
        {
            return this.icaoRegistrations.Where(icao => icao.Countries.Contains(country)).ToList();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a random ICAO registration record.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/07/2021.
        /// </remarks>
        /// <returns>
        /// The random icao registration.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IcaoRegistration GetRandomIcaoRegistration()
        {
            return this.icaoRegistrations[Random.Next(0, this.icaoRegistrations.Length - 1)];
        }
    }
}