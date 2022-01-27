// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flight.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

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
        /// The user operator.
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
        /// The assigned airline pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser assignedAirlinePilot;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport destination;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser dispatcher;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<FlightPayload> flightPayloads;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The "landed at" airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport landedAt;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<FlightNavlogFix> navlogFixes;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The operator airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline operatorAirline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport origin;

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
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Flight ValidEmptyModel =>
            new()
            {
                ID = Guid.Empty,
                Aircraft = Aircraft.ValidEmptyModel,
                Origin = Airport.ValidEmptyModel,
                Destination = Airport.ValidEmptyModel,
                Alternate = Airport.ValidEmptyModel
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AircraftRegistry")]
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
        [JsonIgnore]
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the true airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
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
        [JsonIgnore]
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateRoute { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the altitude in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the assigned airline pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AssignedAirlinePilotID")]
        [JsonIgnore]
        public OpenSkyUser AssignedAirlinePilot
        {
            get => this.LazyLoader.Load(this, ref this.assignedAirlinePilot);
            set => this.assignedAirlinePilot = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the assigned airline pilot (should not be set for user operated flights - will be ignored).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AssignedAirlinePilot")]
        [StringLength(255)]
        [JsonIgnore]
        public string AssignedAirlinePilotID { get; set; }

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
        [JsonIgnore]
        public DateTime? Completed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was created.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [JsonIgnore]
        public DateTime Created { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
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
        [JsonIgnore]
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("DispatcherID")]
        [JsonIgnore]
        public OpenSkyUser Dispatcher
        {
            get => this.LazyLoader.Load(this, ref this.dispatcher);
            set => this.dispatcher = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Dispatcher")]
        [StringLength(255)]
        [JsonIgnore]
        public string DispatcherID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string DispatcherName => this.Dispatcher?.UserName ?? "";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherRemarks { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the final flight log file (base64 encoded).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public string FlightLog { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight number (1-9999).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<FlightPayload> FlightPayloads
        {
            get => this.LazyLoader.Load(this, ref this.flightPayloads);
            set => this.flightPayloads = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight phase (reported by the agent).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightPhase FlightPhase { get; set; }

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
        /// Gets the full flight number (airline code and number combined).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string FullFlightNumber
        {
            get
            {
                var flightNumber = $"{this.FlightNumber}";
                if (!string.IsNullOrEmpty(this.OperatorAirline?.IATA))
                {
                    flightNumber = $"{this.OperatorAirline?.IATA}{flightNumber}";
                }
                else if (!string.IsNullOrEmpty(this.OperatorAirlineID))
                {
                    flightNumber = $"{this.OperatorAirlineID}{flightNumber}";
                }

                return flightNumber;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground speed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? GroundSpeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this flight has an auto-saved log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public bool HasAutoSaveLog => !string.IsNullOrEmpty(this.AutoSaveLog);

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
        /// Gets or sets the "landed at" airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("LandedAtICAO")]
        [JsonIgnore]
        public Airport LandedAt
        {
            get => this.LazyLoader.Load(this, ref this.landedAt);
            set => this.landedAt = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the "landed at" airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        [JsonIgnore]
        public string LandedAtICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the last auto-save.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LastAutoSave { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the last position report.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LastPositionReport { get; set; }

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
        /// Gets or sets the navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<FlightNavlogFix> NavlogFixes
        {
            get => this.LazyLoader.Load(this, ref this.navlogFixes);
            set => this.navlogFixes = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OFP HTML (most likely from simBrief).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OfpHtml { get; set; }

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
        /// Gets or sets the identifier of the airline operator of this flight (either this or OperatorID
        /// must be set.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OperatorAirline")]
        [StringLength(3)]
        [JsonIgnore]
        public string OperatorAirlineID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the operator of this flight (either this or OperatorAirlineID
        /// must be set.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Operator")]
        [StringLength(255)]
        [JsonIgnore]
        public string OperatorID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the flight operator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OperatorName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.OperatorAirlineID))
                {
                    return this.OperatorAirline?.Name ?? this.OperatorAirlineID;
                }

                return this.Operator?.UserName ?? "Unknown";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
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
        [JsonIgnore]
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was paused (so it can be resumed later).
        /// </summary>
        /// 
        public DateTime? Paused { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when payload loading will be complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? PayloadLoadingComplete { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pitch angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PitchAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime PlannedDepartureTime { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Route { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was started (left planning phase).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public DateTime? Started { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the time-warp time saved (in seconds).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TimeWarpTimeSavedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset { get; set; }

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
    }
}