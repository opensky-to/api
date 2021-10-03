// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPlan.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using OpenSky.API.DbModel;

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

            this.ID = flight.ID;
            this.FlightNumber = flight.FlightNumber;
            this.AircraftRegistry = flight.AircraftRegistry;
            this.OriginICAO = flight.OriginICAO;
            this.DestinationICAO = flight.DestinationICAO;
            this.AlternateICAO = flight.AlternateICAO;
            this.FuelGallons = flight.FuelGallons;
            this.UtcOffset = flight.UtcOffset;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(10, MinimumLength = 5)]
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string DestinationICAO { get; set; }

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
        /// Gets or sets the fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double? FuelGallons { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset { get; set; }
    }
}