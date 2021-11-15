// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLog.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;

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
        /// -------------------------------------------------------------------------------------------------
        public FlightLog(Flight flight)
        {
            this.ID = flight.ID;
            this.FullFlightNumber = flight.FullFlightNumber;
            this.AircraftRegistry = flight.AircraftRegistry;
            this.OriginICAO = flight.OriginICAO;
            this.DestinationICAO = flight.DestinationICAO;
            this.AlternateICAO = flight.AlternateICAO;
            this.LandedAtICAO = flight.LandedAtICAO;
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

            var finalFuel = flight.FuelTankCenterQuantity + flight.FuelTankCenter2Quantity + flight.FuelTankCenter3Quantity +
                            flight.FuelTankLeftMainQuantity + flight.FuelTankLeftAuxQuantity + flight.FuelTankLeftTipQuantity +
                            flight.FuelTankRightMainQuantity + flight.FuelTankRightAuxQuantity + flight.FuelTankRightTipQuantity +
                            flight.FuelTankExternal1Quantity + flight.FuelTankExternal2Quantity;

            this.FuelConsumption = (flight.FuelGallons ?? 0.0) - (finalFuel ?? 0);
            this.FuelConsumedWeight = this.FuelConsumption * flight.Aircraft.Type.FuelWeightPerGallon;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateICAO { get; set; }

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
        /// Gets or sets destination airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel consumed weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelConsumedWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel consumption.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelConsumption { get; set; }

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
        /// Gets or sets the "landed at" airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandedAtICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the planned departure time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime PlannedDepartureTime { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the flight was started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Started { get; set; }
    }
}