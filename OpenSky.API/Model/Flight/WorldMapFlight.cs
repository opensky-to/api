// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldMapFlight.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using Microsoft.AspNetCore.Identity;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World map flight.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldMapFlight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldMapFlight"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <param name="flight">
        /// The flight from the db.
        /// </param>
        /// <param name="userManager">
        /// The API user manager.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldMapFlight(Flight flight, UserManager<OpenSkyUser> userManager)
        {
            this.FullFlightNumber = flight.FullFlightNumber;
            this.AircraftRegistry = flight.AircraftRegistry;
            if (!string.IsNullOrEmpty(flight.Aircraft.Name))
            {
                this.AircraftRegistry += $" ({flight.Aircraft.Name})";
            }

            this.AircraftType = flight.Aircraft.Type.Name;
            this.Origin = $"{flight.OriginICAO}: {flight.Origin.Name}";
            this.Destination = $"{flight.DestinationICAO}: {flight.Destination.Name}";
            this.Operator = flight.OperatorName;
            this.Pilot = flight.Operator?.UserName ?? "Unknown";
            if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
            {
                var pilotUser = userManager.FindByIdAsync(flight.AssignedAirlinePilotID).Result;
                this.Pilot = pilotUser != null ? pilotUser.UserName : "Unknown";
            }

            this.FlightPhase = flight.FlightPhase;
            this.Altitude = flight.Altitude ?? 0;
            if (this.Altitude == 0 && this.FlightPhase == FlightPhase.Briefing || this.FlightPhase == FlightPhase.PreFlight)
            {
                this.Altitude = flight.Origin.Altitude;
            }

            this.GroundSpeed = flight.GroundSpeed ?? 0;
            this.Heading = flight.Heading ?? 0;
            this.OnGround = flight.OnGround;
            this.IsPaused = flight.Paused.HasValue;

            if (!flight.Latitude.HasValue || !flight.Longitude.HasValue)
            {
                this.Latitude = flight.Origin.Latitude;
                this.Longitude = flight.Origin.Longitude;
            }
            else
            {
                this.Latitude = flight.Latitude.Value;
                this.Longitude = flight.Longitude.Value;
            }
        }

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
        /// Gets or sets the altitude in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Destination for the.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Destination { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight phase (reported by the agent).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightPhase FlightPhase { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the full flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FullFlightNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground speed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The magnetic heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this flight is paused.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsPaused { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

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
        /// Gets or sets the pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Pilot { get; set; }
    }
}