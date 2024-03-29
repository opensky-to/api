﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeUpgrade.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.AircraftType
{
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type upgrade model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypeUpgrade
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of aircraft affected by this upgrade.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int AircraftCount { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type we are upgrading from.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType From { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type we are upgrading to.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType To { get; set; }
    }
}