// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewAircraftDeliveryOption.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Aircraft
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// New aircraft delivery options.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum NewAircraftDeliveryOption
    {
        /// <summary>
        /// Take delivery at one of the manufacturer's delivery airports.
        /// </summary>
        ManufacturerDeliveryAirport = 0,

        /// <summary>
        /// Have the manufacturer ferry the aircraft for you.
        /// </summary>
        ManufacturerFerry = 1,

        /// <summary>
        /// Outsource the ferry flight(s) to another OpenSky player/airline.
        /// </summary>
        OutsourceFerry = 2
    }
}