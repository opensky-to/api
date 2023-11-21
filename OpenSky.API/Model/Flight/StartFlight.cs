﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartFlight.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;
    using System.Collections.Generic;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Start flight model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class StartFlight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid FlightID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a list of states to override the checks for.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<StartFlightStatus> OverrideStates { get; set; }
    }
}