﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReRegisterAircraft.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Aircraft
{
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Re-register aircraft model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/11/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ReRegisterAircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the old registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string From { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the country the aircraft is being registered in now.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Country InCountry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the new registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string To { get; set; }
    }
}