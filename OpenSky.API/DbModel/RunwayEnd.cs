﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunwayEnd.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Runway-End model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class RunwayEnd
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach light system (NULL for no approach light).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(15)]
        public string ApproachLightSystem { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the runway end has closed markings painted on it.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasClosedMarkings { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the hash code (SHA1 over all data columns to detect if record needs updating).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string HashCode { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the compass heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway end ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the left VASI pitch angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? LeftVasiPitch { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the left VASI (Visual approach slope indicator) type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(15)]
        public string LeftVasiType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name (for example 04L).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the offset threshold in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int OffsetThreshold { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the right VASI pitch angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? RightVasiPitch { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the right VASI (Visual approach slope indicator) type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(15)]
        public string RightVasiType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("RunwayID")]
        [JsonIgnore]
        public Runway Runway { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the parent runway ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Runway")]
        public int RunwayID { get; set; }
    }
}