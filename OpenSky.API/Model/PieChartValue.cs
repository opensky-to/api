﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PieChartValue.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Pie chart value.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class PieChartValue
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Key { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long Value { get; set; }
    }
}