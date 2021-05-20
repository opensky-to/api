// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
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
    /// Aircraft model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Aircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport (current or origin if aircraft currently in flight).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Aircraft"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Aircraft"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport (current or origin if aircraft currently in flight).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public Airport Airport
        {
            get => this.LazyLoader.Load(this, ref this.airport);
            set => this.airport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport ICAO the plane is located at, note this is the departure airport if
        /// the aircraft currently is flying.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(5, MinimumLength = 3)]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user owner (NULL if no user owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OwnerID")]
        [JsonIgnore]
        public OpenSkyUser Owner { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user that owns this aircraft (NULL if no user owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Owner")]
        [StringLength(255)]
        public string OwnerID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [StringLength(10, MinimumLength = 5)]
        public string Registry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}