// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeCategory.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Values that represent aircraft type categories.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum AircraftTypeCategory
    {
        /// <summary>
        /// Single Engine Piston.
        /// </summary>
        SEP = 0,

        /// <summary>
        /// Multi Engine Piston.
        /// </summary>
        MEP = 1,

        /// <summary>
        /// Single Engine Turboprop.
        /// </summary>
        SET = 2,

        /// <summary>
        /// Multi Engine Turboprop.
        /// </summary>
        MET = 3,

        /// <summary>
        /// Jet (small private and business jets).
        /// </summary>
        Jet = 4,

        /// <summary>
        /// Regional Airliner Jets.
        /// </summary>
        Regional = 5,

        /// <summary>
        /// Narrow-Body Airliner.
        /// </summary>
        NBAirliner = 6,

        /// <summary>
        /// Wide-Body Airliner.
        /// </summary>
        WBAirliner = 7,

        /// <summary>
        /// Helicopter.
        /// </summary>
        Helicopter = 8
    }
}