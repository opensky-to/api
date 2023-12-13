// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLog.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;

    using Microsoft.AspNetCore.Identity;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight log model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightLog
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightLog"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightLog()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightLog"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// <param name="flight">
        /// The flight from the db.
        /// </param>
        /// <param name="userManager">
        /// The API user manager.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FlightLog(Flight flight, UserManager<OpenSkyUser> userManager)
        {
            this.ID = flight.ID;
            this.FullFlightNumber = flight.FullFlightNumber;
            this.AircraftRegistry = flight.AircraftRegistry;
            if (!string.IsNullOrEmpty(flight.Aircraft.Name))
            {
                this.AircraftRegistry += $" ({flight.Aircraft.Name})";
            }

            this.AircraftType = flight.Aircraft.Type.Name;
            this.Category = flight.Aircraft.Type.Category;
            this.Simulator = flight.Aircraft.Type.Simulator;
            this.AircraftEngineType = flight.Aircraft.Type.EngineType;
            this.OriginICAO = flight.OriginICAO;
            this.Origin = flight.Origin.Name;
            this.DestinationICAO = flight.DestinationICAO;
            this.Destination = flight.Destination.Name;
            this.AlternateICAO = flight.AlternateICAO;
            this.Alternate = flight.Alternate.Name;
            this.LandedAtICAO = flight.LandedAtICAO;
            this.LandedAt = flight.LandedAt.Name;
            this.PlannedDepartureTime = flight.PlannedDepartureTime;
            if (flight.Started != null)
            {
                // Should always be there, but better safe than sorry
                this.Started = flight.Started.Value;
            }

            if (flight.Completed != null)
            {
                // Should always be there, but better safe than sorry
                this.Completed = flight.Completed.Value;
            }

            this.IsAirlineFlight = !string.IsNullOrEmpty(flight.OperatorAirlineID);
            this.Crashed = flight.FlightPhase == FlightPhase.Crashed;

            this.OffBlockFuel = flight.FuelGallons ?? 0;
            var finalFuel = flight.FuelTankCenterQuantity + flight.FuelTankCenter2Quantity + flight.FuelTankCenter3Quantity +
                            flight.FuelTankLeftMainQuantity + flight.FuelTankLeftAuxQuantity + flight.FuelTankLeftTipQuantity +
                            flight.FuelTankRightMainQuantity + flight.FuelTankRightAuxQuantity + flight.FuelTankRightTipQuantity +
                            flight.FuelTankExternal1Quantity + flight.FuelTankExternal2Quantity;
            this.OnBlockFuel = finalFuel ?? 0;

            this.FuelConsumption = (flight.FuelGallons ?? 0.0) - (finalFuel ?? 0);
            this.FuelWeightPerGallon = flight.Aircraft.Type.FuelWeightPerGallon;
            this.TimeWarpTimeSavedSeconds = flight.TimeWarpTimeSavedSeconds;
            this.Route = flight.Route;
            this.AlternateRoute = flight.AlternateRoute;
            this.Operator = flight.OperatorName;
            this.Pilot = flight.Operator?.UserName ?? "Unknown";
            if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
            {
                var pilotUser = userManager.FindByIdAsync(flight.AssignedAirlinePilotID).Result;
                this.Pilot = pilotUser != null ? pilotUser.UserName : "Unknown";
            }

            this.Dispatcher = flight.DispatcherName;
            this.DispatcherRemarks = flight.DispatcherRemarks;
            this.OnlineNetwork = flight.OnlineNetwork;
            this.AtcCallsign = flight.AtcCallsign;
            this.OnlineNetworkConnectedSeconds = flight.OnlineNetworkConnectedSeconds;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the aircraft engine.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public EngineType AircraftEngineType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AircraftType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Alternate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate route.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateRoute { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the atc callsign.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AtcCallsign { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeCategory Category { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was completed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Completed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft crashed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Crashed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Destination for the.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Destination { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Dispatcher { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatcher remarks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DispatcherRemarks { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel consumption.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelConsumption { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel weight per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeightPerGallon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the full flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FullFlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this is an airline flight or a private one.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsAirlineFlight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the landed at.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAt { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the "landed at" airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAtICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the off block fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double OffBlockFuel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the on block fuel.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double OnBlockFuel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the online network.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public OnlineNetwork OnlineNetwork { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the online network connected seconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int OnlineNetworkConnectedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Operator { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Origin { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Pilot { get; set; }

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
        /// Gets or sets the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Simulator Simulator { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Started { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the time-warp time saved (in seconds).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TimeWarpTimeSavedSeconds { get; set; }
    }
}