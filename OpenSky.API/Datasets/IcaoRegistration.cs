﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IcaoRegistration.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Maps the first two letters of a ICAO Airport code to a registration prefix
    /// </summary>
    public class IcaoRegistration
    {
        /// <summary>
        /// First two chars of the airports ICAO
        /// </summary>
        [Name("AirportPrefix")]
        public string AirportPrefix { get; set; }

        /// <summary>
        /// Full name of the airports country
        /// </summary>
        [Name("Country")]
        public string Country { get; set; }

        /// <summary>
        /// Prefixes used for registrations.
        /// Its the raw string read from the CSV
        /// You are probably looking for 
        /// <see cref="AircraftPrefixes"/>
        /// </summary>
        [Optional]
        [Name("AircraftPrefix")]
        public string AircraftPrefix { get; set; }

        /// <summary>
        /// Returns the raw string from the CSV as string array split along the separator.
        /// </summary>
        public string[] AircraftPrefixes => this.AircraftPrefix.Split(";");
    }
}