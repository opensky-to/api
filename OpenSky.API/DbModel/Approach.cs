// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Approach.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.Helpers;

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