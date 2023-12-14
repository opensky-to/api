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
        /// Instrument
        /// </summary>
        IFR = 0,

        /// <summary>
        /// Visual
        /// </summary>
        VFR = 1,

        /// <summary>
        /// Instrument -> Visual
        /// </summary>
        IFRtoVFR = 2,

        /// <summary>
        /// Visual -> Instrument
        /// </summary>
        VFRtoIFR = 3
    }
}