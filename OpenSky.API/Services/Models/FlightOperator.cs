// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightOperator.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight operator types.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FlightOperator
    {
        /// <summary>
        /// Player completed flight.
        /// </summary>
        Player,

        /// <summary>
        /// Airline completed flight.
        /// </summary>
        Airline
    }
}