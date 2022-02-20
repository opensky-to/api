// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftManufacturerDeliveryLocation.cs" company="OpenSky">
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

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft manufacturer delivery location model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftManufacturerDeliveryLocation
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType aircraftType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftManufacturer manufacturer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftManufacturerDeliveryLocation"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturerDeliveryLocation()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftManufacturerDeliveryLocation"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="lazyLoader">
        /// Gets the lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturerDeliveryLocation(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AircraftTypeID")]
        [JsonIgnore]
        public AircraftType AircraftType
        {
            get => this.LazyLoader.Load(this, ref this.aircraftType);
            set => this.aircraftType = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [ForeignKey("AircraftType")]
        public Guid AircraftTypeID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AirportICAO")]
        [JsonIgnore]
        public Airport Airport
        {
            get => this.LazyLoader.Load(this, ref this.airport);
            set => this.airport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(5, MinimumLength = 3)]
        [ForeignKey("Airport")]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("ManufacturerID")]
        [JsonIgnore]
        public AircraftManufacturer Manufacturer
        {
            get => this.LazyLoader.Load(this, ref this.manufacturer);
            set => this.manufacturer = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(5, MinimumLength = 3)]
        [ForeignKey("Manufacturer")]
        public string ManufacturerID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}