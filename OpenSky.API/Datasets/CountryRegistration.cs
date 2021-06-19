// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountryRegistration.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Datasets
{
    /// <summary>
    /// Maps a Alpha 2 country code to a registration prefix
    /// </summary>
    public class CountryRegistration
    {
        public string iso_country { get; set; }
        public string prefix { get; set; }

        public string alt_prefix { get; set; }
    }
}