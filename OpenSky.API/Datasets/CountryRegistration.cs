// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountryRegistration.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Maps a Alpha 2 country code to a registration prefix
    /// </summary>
    public class CountryRegistration
    {
        /// <summary>
        /// Alpha 2 country code.
        /// </summary>
        [Name("iso_country")]
        public string IsoCountry { get; set; }

        /// <summary>
        /// Primary prefix for registrations.
        /// </summary>
        [Name("prefix")]
        public string prefix { get; set; }

        /// <summary>
        /// Alternative prefix for registrations.
        /// </summary>
        [Optional]
        [Name("alt_prefix")]
        public string AltPrefix { get; set; }
    }
}