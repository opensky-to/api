// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportData.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    /// <summary>
    /// Basic structure of airports.csv from ourairports.
    /// Does not include all fields, only the relevant ones.
    /// </summary>
    public class AirportData
    {
        public int id { get; set; }
        public string ident { get; set; }
        public string iso_country { get; set; }
    }
}