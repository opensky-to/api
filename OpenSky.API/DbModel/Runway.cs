// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Runway.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Runway model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Runway
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Runway"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Runway()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Runway"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Runway(Action<object, string> lazyLoader)
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
        /// Gets or sets the altitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The type of center lighting (NULL for no lighting).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(1)]
        public string CenterLight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The type of available edge lighting (NULL for no lighting).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(1)]
        public string EdgeLight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the hash code (SHA1 over all data columns to detect if record needs updating).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string HashCode { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the length of the runway in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Length { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway ends.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<RunwayEnd> RunwayEnds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the surface type (can be "UNKNOWN" in a few cases).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(7)]
        public string Surface { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the width of the runway in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Width { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}