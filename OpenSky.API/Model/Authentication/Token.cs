﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Token.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Token model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Token
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the expiry of the token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Expiry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the token (ex. website, agent-msfs, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the geo location (country) the token was created from.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string TokenGeo { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the IP address the token was created from.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string TokenIP { get; set; }
    }
}