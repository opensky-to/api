// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Approach.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.Helpers;

    /*
     * APPROACH EXAMPLE RECORD FROM DB (LOWW - Vienna International)
     *
     * INSERT INTO `Approaches` (`ID`, `AirportICAO`, `RunwayName`, `Suffix`, `Type`)
     *
     * VALUES ('12541', 'LOWW', '16', NULL, 'VORDME'),
     *        ('12542', 'LOWW', '34', NULL, 'VORDME'),
     *        ('12543', 'LOWW', '11', NULL, 'ILS'),
     *        ('12544', 'LOWW', '16', NULL, 'ILS'),
     *        ('12545', 'LOWW', '29', NULL, 'ILS'),
     *        ('12546', 'LOWW', '34', NULL, 'ILS'),
     *        ('12547', 'LOWW', '11', NULL, 'LOC'),
     *        ('12548', 'LOWW', '16', NULL, 'LOC'),
     *        ('12549', 'LOWW', '29', NULL, 'LOC'),
     *        ('12550', 'LOWW', '34', NULL, 'LOC'),
     *        ('12551', 'LOWW', '11', NULL, 'NDBDME'),
     *        ('12552', 'LOWW', '29', NULL, 'NDBDME'),
     *        ('12553', 'LOWW', '11', 'Z', 'RNAV'),
     *        ('12554', 'LOWW', '16', 'E', 'RNAV'),
     *        ('12555', 'LOWW', '16', 'N', 'RNAV'),
     *        ('12556', 'LOWW', '16', 'Z', 'RNAV'),
     *        ('12557', 'LOWW', '29', 'X', 'RNAV'),
     *        ('12558', 'LOWW', '29', 'Z', 'RNAV'),
     *        ('12559', 'LOWW', '34', NULL, 'RNAV')
     */

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Approach model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Approach
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Approach"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Approach()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Approach"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Approach(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("AirportICAO")]
        public Airport Airport
        {
            get => this.LazyLoader.Load(this, ref this.airport);
            set => this.airport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport ICAO.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        [Required]
        [ForeignKey("Airport")]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the runway (can be NULL if approach doesn't specify a runway).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(6)]
        public string RunwayName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach type suffix (Y, Z, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(1)]
        public string Suffix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach type (ILS, RNAV, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(25)]
        public string Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}