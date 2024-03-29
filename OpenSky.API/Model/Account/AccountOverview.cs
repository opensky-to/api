﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountOverview.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Account
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Account overview model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AccountOverview
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the airline (if the user is a member of one).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AirlineName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the user joined.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Joined { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public byte[] ProfileImage { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the token renewal country verification is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool TokenRenewalCountryVerification { get; set; }
    }
}