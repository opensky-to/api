// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Airport.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Extensions.AirportsJSON
{
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport extension to quickly create JSON package airport.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AirportExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An Airport extension method that constructs an airports JSON airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/12/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to act on.
        /// </param>
        /// <returns>
        /// An OpenSky.AirportsJSON.Airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSky.AirportsJSON.Airport ConstructAirportsJson(this Airport airport)
        {
            return new OpenSky.AirportsJSON.Airport
            {
                ICAO = airport.ICAO,
                Name = airport.Name,
                Latitude = airport.Latitude,
                Longitude = airport.Longitude,
                Size = airport.Size ?? -1,
                HasAvGas = airport.HasAvGas,
                HasJetFuel = airport.HasJetFuel,
                IsClosed = airport.IsClosed,
                IsMilitary = airport.IsMilitary,
                SupportsSuper = airport.SupportsSuper,
                MSFS = airport.MSFS
            };
        }
    }
}