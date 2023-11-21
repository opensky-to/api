// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartFlightResult.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Start flight status.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum StartFlightStatus
    {
        /// <summary>
        /// A non-overridable error has occurred and the flight cannot start.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Flight started successfully.
        /// </summary>
        Started = 0,

        /// <summary>
        /// The aircraft is not at the origin airport.
        /// </summary>
        AircraftNotAtOrigin = 1,

        /// <summary>
        /// The origin airport doesn't sell AvGas.
        /// </summary>
        OriginDoesntSellAvGas = 2,

        /// <summary>
        /// The origin airport doesn't sell JetFuel.
        /// </summary>
        OriginDoesntSellJetFuel = 3,

        /// <summary>
        /// Payloads found aboard the aircraft that are not on the flight plan.
        /// </summary>
        NonFlightPlanPayloadsFound = 4
    }
}