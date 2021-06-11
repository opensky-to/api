// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeCategory.cs" company="OpenSky">
// sushi.at for OpenSky 2021
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
        /// Narrow-Body Airliner.
        /// </summary>
        NBAirliner = 5,

        /// <summary>
        /// Wide-Body Airliner.
        /// </summary>
        WBAirliner = 6
    }
}