﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateAircraft.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Aircraft
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Update aircraft model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UpdateAircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user-chosen name of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(30)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the purchase price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? PurchasePrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the registry of the aircraft to update.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string Registry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the rent price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? RentPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the variant (can be set to Guid.Empty to leave unchanged).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid VariantID { get; set; }
    }
}