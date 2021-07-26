// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
// OpenSky project 2021
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
        /// The aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType type;

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
        [ForeignKey("AirportICAO")]
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
        [ForeignKey("Airport")]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Fuel { get; set; }

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
        /// Gets the owner name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string OwnerName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.OwnerID))
                {
                    return this.Owner.UserName;
                }

                // todo return VA owner once we add that

                return "[System]";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the purchase price for the aircraft. Null if not available for purchase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? PurchasePrice { get; set; }

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
        /// Gets or sets the rent price per flight hour for the aircraft. Null if not available for rent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? RentPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("TypeID")]
        public AircraftType Type
        {
            get => this.LazyLoader.Load(this, ref this.type);
            set => this.type = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Type")]
        public Guid TypeID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}