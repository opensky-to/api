﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevokeTokenByName.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Revoke token by name and expiry model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class RevokeTokenByName
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the expiry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Expiry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }
    }
}