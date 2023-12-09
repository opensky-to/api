// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aircraft.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
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
        /// The airline owner of the aircraft (or NULL if no airline owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline airlineOwner;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport (current or origin if aircraft currently in flight).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flights of this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> flights;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The maintenance records for this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftMaintenance> maintenanceRecords;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The owner of the aircraft (or NULL if no user owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser owner;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payloads currently loaded onto this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Payload> payloads;

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
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Aircraft ValidEmptyModel =>
            new()
            {
                AirportICAO = "XXXX",
                Registry = "XXXX",
                Type = AircraftType.ValidEmptyModel
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft maintenance records for this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AircraftMaintenance> MaintenanceRecords
        {
            get => this.LazyLoader.Load(this, ref this.maintenanceRecords);
            set => this.maintenanceRecords = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airframe hours.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirframeHours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline owner (NULL if no airline owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AirlineOwnerID")]
        [JsonIgnore]
        public Airline AirlineOwner
        {
            get => this.LazyLoader.Load(this, ref this.airlineOwner);
            set => this.airlineOwner = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the airline owner (NULL if no airline owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AirlineOwnerID")]
        [StringLength(3)]
        [ConcurrencyCheck]
        public string AirlineOwnerID { get; set; }

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
        /// Can the aircraft currently start a new flight?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public bool CanStartFlight
        {
            get
            {
                if (this.Flights != null)
                {
                    // Check for active flight
                    var activeFlight = this.Flights.SingleOrDefault(f => f.Started.HasValue && !f.Completed.HasValue);
                    if (activeFlight != null)
                    {
                        return false;
                    }

                    if (this.WarpingUntil.HasValue && this.WarpingUntil.Value > DateTime.UtcNow)
                    {
                        return false;
                    }

                    // todo return repair/etc. status

                    return true;
                }

                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the engine 1 hours.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Engine1Hours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the engine 2 hours.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Engine2Hours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the engine 3 hours.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Engine3Hours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the engine 4 hours.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Engine4Hours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<Flight> Flights
        {
            get => this.LazyLoader.Load(this, ref this.flights);
            set => this.flights = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Fuel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time until the aircraft is fuelling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? FuellingUntil { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current heading of the aircraft, or 0 if not available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public double Heading
        {
            get
            {
                var activeFlight = this.Flights?.SingleOrDefault(f => f.Started.HasValue && !f.Completed.HasValue);
                if (activeFlight != null)
                {
                    return activeFlight.Heading ?? 0;
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public double Latitude
        {
            get
            {
                var activeFlight = this.Flights?.SingleOrDefault(f => f.Started.HasValue && !f.Completed.HasValue);
                if (activeFlight != null)
                {
                    return activeFlight.Latitude ?? activeFlight.Origin.Latitude;
                }

                return this.Airport?.Latitude ?? 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the life time expense (for the current owner only).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long LifeTimeExpense { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the life time income (for the current owner only).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long LifeTimeIncome { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time until the aircraft is loading payload (cargo or pax).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LoadingUntil { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public double Longitude
        {
            get
            {
                var activeFlight = this.Flights?.SingleOrDefault(f => f.Started.HasValue && !f.Completed.HasValue);
                if (activeFlight != null)
                {
                    return activeFlight.Longitude ?? activeFlight.Origin.Longitude;
                }

                return this.Airport?.Longitude ?? 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user-chosen name of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(30)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user owner (NULL if no user owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("OwnerID")]
        [JsonIgnore]
        public OpenSkyUser Owner
        {
            get => this.LazyLoader.Load(this, ref this.owner);
            set => this.owner = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user that owns this aircraft (NULL if no user owner).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Owner")]
        [StringLength(255)]
        [ConcurrencyCheck]
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
                    return this.Owner?.UserName ?? "???";
                }

                if (!string.IsNullOrEmpty(this.AirlineOwnerID))
                {
                    return this.AirlineOwner?.Name ?? "???";
                }

                return "[System]";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payloads currently loaded onto this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [InverseProperty("Aircraft")]
        public ICollection<Payload> Payloads
        {
            get => this.LazyLoader.Load(this, ref this.payloads);
            set => this.payloads = value;
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
        [StringLength(12, MinimumLength = 6)]
        [Required]
        public string Registry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the rent price per flight hour for the aircraft. Null if not available for rent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? RentPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current status of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string Status
        {
            get
            {
                if (this.Flights != null)
                {
                    // Check for active flight
                    var activeFlight = this.Flights.SingleOrDefault(f => f.Started.HasValue && !f.Completed.HasValue);
                    if (activeFlight != null)
                    {
                        if (activeFlight.Paused.HasValue)
                        {
                            return $"Paused ({activeFlight.FullFlightNumber})";
                        }

                        return $"{activeFlight.FlightPhase} ({activeFlight.FullFlightNumber})";
                    }

                    if (this.WarpingUntil.HasValue && this.WarpingUntil.Value > DateTime.UtcNow)
                    {
                        return $"Warping T-{(DateTime.UtcNow - this.WarpingUntil.Value):hh\\:mm\\:ss}";
                    }

                    if (this.FuellingUntil.HasValue && this.FuellingUntil.Value > DateTime.UtcNow && this.LoadingUntil.HasValue && this.LoadingUntil.Value > DateTime.UtcNow)
                    {
                        var maxTime = this.FuellingUntil.Value > this.LoadingUntil.Value ? this.FuellingUntil.Value : this.LoadingUntil.Value;
                        return $"Ground handling T-{(DateTime.UtcNow - maxTime):hh\\:mm\\:ss}";
                    }

                    if (this.FuellingUntil.HasValue && this.FuellingUntil.Value > DateTime.UtcNow)
                    {
                        return $"Fuelling T-{(DateTime.UtcNow - this.FuellingUntil.Value):hh\\:mm\\:ss}";
                    }

                    if (this.LoadingUntil.HasValue && this.LoadingUntil.Value > DateTime.UtcNow)
                    {
                        return $"Loading T-{(DateTime.UtcNow - this.LoadingUntil.Value):hh\\:mm\\:ss}";
                    }

                    // todo return repair/etc. status

                    return "Idle";
                }

                return "???";
            }
        }

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
        [Required]
        public Guid TypeID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time until the aircraft is warping.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? WarpingUntil { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}