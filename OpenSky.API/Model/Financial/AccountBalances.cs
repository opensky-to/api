﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountBalances.cs" company="OpenSky">
// OpenSky project 2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Financial
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Account balances model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AccountBalances
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the account balance (of the user).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long AccountBalance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline account balance (if user has the permission).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long? AirlineAccountBalance { get; set; }
    }
}