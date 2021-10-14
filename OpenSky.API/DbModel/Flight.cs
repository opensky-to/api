﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flight.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Flight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------

        // ReSharper disable once InconsistentNaming
        private OpenSkyUser _operator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft aircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport alternate;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport destination;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport origin;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the operator airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline operatorAirline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Flight"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/09/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Flight()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Flight"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/09/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Flight(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AircraftRegistry")]
        [JsonIgnore]
        public Aircraft Aircraft
        {
            get => this.LazyLoader.Load(this, ref this.aircraft);
            set => this.aircraft = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Aircraft")]
        [StringLength(10, MinimumLength = 5)]
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("AlternateICAO")]
        public Airport Alternate
        {
            get => this.LazyLoader.Load(this, ref this.alternate);
            set => this.alternate = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latest auto-save flight log file (base64 encoded).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public string AutoSaveLog { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was completed (landed, crashed, aborted or otherwise).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Completed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was paused (so it can be resumed later).
        /// </summary>
        /// 
        public DateTime? Paused { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was created.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public DateTime Created { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("DestinationICAO")]
        public Airport Destination
        {
            get => this.LazyLoader.Load(this, ref this.destination);
            set => this.destination = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the final flight log file (base64 encoded).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public string FlightLog { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(7, MinimumLength = 1)]
        [Required]
        public string FlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight phase (reported by the agent).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightPhase FlightPhase { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankCenter2Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 3 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankCenter3Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankCenterQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 1 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankExternal1Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankExternal2Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankLeftAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankLeftMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankLeftTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankRightAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankRightMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelTankRightTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The magnetic heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the operator of this flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OperatorID")]
        [JsonIgnore]
        public OpenSkyUser Operator
        {
            get => this.LazyLoader.Load(this, ref this._operator);
            set => this._operator = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the operator of this flight (either this or OperatorAirlineID
        /// must be set.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Operator")]
        [StringLength(255)]
        public string OperatorID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the airline operator of this flight (either this or OperatorID
        /// must be set.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OperatorAirline")]
        [StringLength(3)]
        public string OperatorAirlineID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the operator airline of this flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OperatorAirlineID")]
        [JsonIgnore]
        public Airline OperatorAirline
        {
            get => this.LazyLoader.Load(this, ref this.operatorAirline);
            set => this.operatorAirline = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("OriginICAO")]
        public Airport Origin
        {
            get => this.LazyLoader.Load(this, ref this.origin);
            set => this.origin = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pitch angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PitchAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was started (left planning phase).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Started { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed in feet per second.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double VerticalSpeedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelGallons { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when fuel loading will be complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? FuelLoadingComplete { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when payload loading will be complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? PayloadLoadingComplete { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset { get; set; }
    }
}