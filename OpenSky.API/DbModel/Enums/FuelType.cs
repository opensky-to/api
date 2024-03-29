﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelType.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel types (derived from engine type, SimConnect doesn't report this).
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FuelType
    {
        /// <summary>
        /// Aviation Gas, only used by piston engines.
        /// </summary>
        AvGas = 0,

        /// <summary>
        /// Jet fuel.
        /// </summary>
        JetFuel = 1,

        /// <summary>
        /// No fuel.
        /// </summary>
        None = 2,

        /// <summary>
        /// Hydrogen fuel cell.
        /// </summary>
        Hydrogen = 3,

        /// <summary>
        /// Battery powered, charged with electricity.
        /// </summary>
        Electricity = 4,

        /// <summary>
        /// Not used, used for override column - instead of null.
        /// </summary>
        NotUsed = 99,
    }
}