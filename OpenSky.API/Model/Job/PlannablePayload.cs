// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlannablePayload.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Job
{
    using System;
    using System.Collections.Generic;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// (Flight)Plannable payload.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class PlannablePayload
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PlannablePayload"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public PlannablePayload(Payload payload)
        {
            this.ID = payload.ID;
            this.DestinationICAO = payload.DestinationICAO;
            this.Weight = payload.Weight;
            this.Description = payload.Description;
            this.CurrentLocation = !string.IsNullOrEmpty(payload.AirportICAO) ? payload.AirportICAO : (!string.IsNullOrEmpty(payload.AircraftRegistry) ? payload.AircraftRegistry : "???");
            this.Flights = new Dictionary<Guid, string>();
            this.Destinations = new List<string>();
            foreach (var flightPayload in payload.FlightPayloads)
            {
                var origin = !string.IsNullOrEmpty(flightPayload.Flight.OriginICAO) ? flightPayload.Flight.OriginICAO : "??";
                var destination = !string.IsNullOrEmpty(flightPayload.Flight.DestinationICAO) ? flightPayload.Flight.DestinationICAO : "??";
                var aircraft = !string.IsNullOrEmpty(flightPayload.Flight.AircraftRegistry) ? flightPayload.Flight.AircraftRegistry : "??";
                var infoString = $"{flightPayload.Flight.FullFlightNumber}: {origin} ▷ {destination} [{aircraft}]";
                this.Flights.Add(flightPayload.FlightID, infoString);

                if (!string.IsNullOrEmpty(flightPayload.Flight.DestinationICAO) && !this.Destinations.Contains(flightPayload.Flight.DestinationICAO))
                {
                    this.Destinations.Add(flightPayload.Flight.DestinationICAO);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current location (airport or aircraft).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string CurrentLocation { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Description { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flights the payload is planned to fly on.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<Guid, string> Flights { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destinations this payload is planning to go to.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> Destinations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight { get; set; }
    }
}