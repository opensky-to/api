// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftManufacturer.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft manufacturer model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftManufacturer
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static AircraftManufacturer ValidEmptyModel =>
            new()
            {
                ID = "XXX",
                Name = "XXXX",
                DeliveryLocations = new List<AircraftManufacturerDeliveryLocation>()
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftManufacturer"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturer()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft types of this manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftType> types;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft types of this manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AircraftType> Types
        {
            get => this.LazyLoader.Load(this, ref this.types);
            set => this.types = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The delivery locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftManufacturerDeliveryLocation> deliveryLocations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the delivery locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AircraftManufacturerDeliveryLocation> DeliveryLocations
        {
            get => this.LazyLoader.Load(this, ref this.deliveryLocations);
            set => this.deliveryLocations = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftManufacturer"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturer(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Short identifier string for the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [StringLength(5, MinimumLength = 3)]
        [Required]
        public string ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Full name of manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
    }
}