// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IcaoRegistration.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    using System;
    using System.Linq;

    using CsvHelper.Configuration.Attributes;

    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Maps the first two letters of a ICAO Airport code to a registration prefix.
    /// </summary>
    /// <remarks>
    /// Flusinerd, 25/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class IcaoRegistration
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prefixes used for registrations. Its the raw string read from the CSV You are probably
        /// looking for
        /// <see cref="AircraftPrefixes"/>
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Optional]
        [Name("AircraftPrefix")]
        public string AircraftPrefix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the raw string from the CSV as string array split along the separator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string[] AircraftPrefixes => this.AircraftPrefix.Split(';');

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// First two chars of the airports ICAO.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Name("AirportPrefix")]
        public string AirportPrefix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the list of countries for this ICAO registration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Country[] Countries => this.Country.Split(';').Select(Enum.Parse<Country>).ToArray();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Two letter country code(s) according to ISO-3166.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Name("Country")]
        public string Country { get; set; }
    }
}