// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightRule.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable InconsistentNaming

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight rules.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FlightRule
    {
        /// <summary>
        /// Instrument flight rules
        /// </summary>
        IFR = 0,

        /// <summary>
        /// Visual flight rules
        /// </summary>
        VFR = 1
    }
}