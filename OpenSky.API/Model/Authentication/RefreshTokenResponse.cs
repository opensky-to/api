﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefreshTokenResponse.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Refresh token response model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class RefreshTokenResponse
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the expiration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Expiration { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RefreshToken { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the refresh token expiration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime RefreshTokenExpiration { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Token { get; set; }
    }
}