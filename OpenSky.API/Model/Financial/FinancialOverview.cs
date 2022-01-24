// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialOverview.cs" company="OpenSky">
// OpenSky project 2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Financial
{
    using System.Collections.Generic;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Financial overview model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FinancialOverview
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
        public long AirlineAccountBalance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the recent airline financial records (if user has the permission).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<FinancialRecord> RecentAirlineFinancialRecords { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the recent financial records (of the user).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<FinancialRecord> RecentFinancialRecords { get; set; }
    }
}