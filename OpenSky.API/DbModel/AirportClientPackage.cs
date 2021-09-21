// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackage.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport client package model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackage
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the creation time of the package.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        public DateTime CreationTime { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the package contents (g-zipped, base64 encoded).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string Package { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the package hash (SHA-256, base64 encoded)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string PackageHash { get; set; }
    }
}