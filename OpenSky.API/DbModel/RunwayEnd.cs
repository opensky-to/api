// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunwayEnd.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    /*
     * RUNWAY-END EXAMPLE RECORD FROM DB (LOWW - Vienna International)
     *
     * INSERT INTO `RunwayEnds` (`ID`, `ApproachLightSystem`, `HasClosedMarkings`, `HashCode`, `Heading`, `Latitude`, `LeftVasiPitch`, `LeftVasiType`, `Longitude`, `Name`, `OffsetThreshold`, `RightVasiPitch`, `RightVasiType`, `RunwayID`)
     *
     * VALUES ('87881', 'ALSF1', '0', 'bc8eb2c524c7ba98fde2e538d7eccdcf0bc5d079', '115.95314025878906', '48.122825622558594', '3', 'PAPI4', '16.53326416015625', '11', '0', NULL, NULL, '43941'),
     *        ('87882', 'ALSF2', '0', '58d7af94775abb0217a6f8d97448e938af52c0f2', '295.953125', '48.10904312133789', '3', 'PAPI4', '16.57568359375', '29', '0', NULL, NULL, '43941'),
     *        ('87883', 'ALSF2', '0', 'f50609081d7ea45be66cda31d427886563c99209', '164.1750030517578', '48.11979293823242', '3', 'PAPI4', '16.578155517578125', '16', '0', NULL, NULL, '43942'),
     *        ('87884', 'ALSF2', '0', '47373c2f3a2c9bc1baa1962a16734c09e6e7017b', '344.17498779296875', '48.088626861572266', '3', 'PAPI4', '16.59136962890625', '34', '0', NULL, NULL, '43942')
     */

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