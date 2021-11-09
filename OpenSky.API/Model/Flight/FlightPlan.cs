// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight plan model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightPlan
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlan"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlan()
        {
            this.NavlogFixes = new List<FlightNavlogFix>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlan"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
        /// </remarks>
        /// <param name="flight">
        /// The flight from the database.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FlightPlan(Flight flight)
        {
            if (flight.Started.HasValue)
            {
                throw new Exception("Can't create flight plan for active flight.");
            }

            if (string.IsNullOrEmpty(flight.OperatorID) && string.IsNullOrEmpty(flight.OperatorAirlineID))
            {
                throw new Exception("Flight plan operator missing.");
            }

            if (!string.IsNullOrEmpty(flight.OperatorID) && !string.IsNullOrEmpty(flight.OperatorAirlineID))
            {
                throw new Exception("Flight plan operator is ambiguous.");
            }

            this.ID = flight.ID;
            this.FlightNumber = flight.FlightNumber;
            this.Aircraft = flight.Aircraft ?? new Aircraft
            {
                Registry = string.Empty,
                AirportICAO = string.Empty,
                TypeID = Guid.Empty,
                Type = new AircraftType
                {
                    AtcModel = string.Empty,
                    AtcType = string.Empty,
                    Category = AircraftTypeCategory.SEP,
                    EngineType = EngineType.None,
                    Manufacturer = string.Empty,
                    Name = string.Empty,
                    UploaderID = string.Empty,
                    Simulator = Simulator.MSFS
                }
            };
            this.OriginICAO = flight.OriginICAO;
            this.DestinationICAO = flight.DestinationICAO;
            this.AlternateICAO = flight.AlternateICAO;
            this.FuelGallons = flight.FuelGallons;
            this.UtcOffset = flight.UtcOffset;
            this.IsAirlineFlight = !string.IsNullOrEmpty(flight.OperatorAirlineID);
            this.PlannedDepartureTime = flight.PlannedDepartureTime;
            this.DispatcherID = flight.DispatcherID;
            this.DispatcherName = flight.Dispatcher?.UserName ?? "";
            this.DispatcherRemarks = flight.DispatcherRemarks;
            this.FullFlightNumber = flight.FullFlightNumber;
            this.Route = flight.Route;
            this.AlternateRoute = flight.AlternateRoute;
            this.OfpHtml = flight.OfpHtml;
            this.NavlogFixes = flight.NavlogFixes ?? new List<FlightNavlogFix>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Aircraft Aircraft { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateRoute { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(255)]
        public string DispatcherID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the dispatcher (read only, for display in list view, not for editing!).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherRemarks { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight number (1-9999).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelGallons { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the full flight number (airline code and number combined)(read only, for display in list view, not for editing!).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FullFlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this is an airline flight or a private one.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAirlineFlight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<FlightNavlogFix> NavlogFixes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OFP HTML (most likely from simBrief).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OfpHtml { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime PlannedDepartureTime { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Route { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset { get; set; }
    }
}