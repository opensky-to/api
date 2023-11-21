// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeCategory.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    using System.ComponentModel;

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
        [Description("Single Engine Piston")]
        SEP = 0,

        /// <summary>
        /// Multi Engine Piston.
        /// </summary>
        [Description("Multi Engine Piston")]
        MEP = 1,

        /// <summary>
        /// Single Engine Turboprop.
        /// </summary>
        [Description("Single Engine Turboprop")]
        SET = 2,

        /// <summary>
        /// Multi Engine Turboprop.
        /// </summary>
        [Description("Multi Engine Turboprop")]
        MET = 3,

        /// <summary>
        /// Small private and business jets.
        /// </summary>
        [Description("Small private and business jets")]
        JET = 4,

        /// <summary>
        /// Regional Airliner Jets.
        /// </summary>
        [Description("Regional Airliner Jets")]
        REG = 5,

        /// <summary>
        /// Narrow-Body Airliner.
        /// </summary>
        [Description("Narrow-Body Airliner")]
        NBA = 6,

        /// <summary>
        /// Wide-Body Airliner.
        /// </summary>
        [Description("Wide-Body Airliner")]
        WBA = 7,

        /// <summary>
        /// Helicopter.
        /// </summary>
        [Description("Helicopter")]
        HEL = 8
    }
}