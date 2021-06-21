// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportData.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Basic structure of airports.csv from ourairports.
    /// Does not include all fields, only the relevant ones.
    /// </summary>,
    public class AirportData
    {
        /// <summary>
        /// ID of the airport.
        /// </summary>
        [Name("id")]
        public int ID { get; set; }

        /// <summary>
        /// icao ident of the airport
        /// </summary>
        [Name("ident")]
        public string Ident { get; set; }
        /// <summary>
        /// Alpha 2 country code of the airport.
        /// </summary>
        [Name("iso_country")]
        public string IsoCountry { get; set; }
    }
}