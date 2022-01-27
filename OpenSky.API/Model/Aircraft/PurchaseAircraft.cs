// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PurchaseAircraft.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Aircraft
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Purchase aircraft model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class PurchaseAircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to purchase the aircraft for the airline or the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ForAirline { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string Registry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the variant (can be Guid.Empty).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid VariantID { get; set; }
    }
}