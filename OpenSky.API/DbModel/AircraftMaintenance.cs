// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftMaintenance.cs" company="OpenSky">
// OpenSky project 2021-2023
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
    /// Aircraft maintenance model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/05/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftMaintenance
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft aircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The planned at airport - or NULL for next available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport plannedAtAirport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftMaintenance"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/05/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftMaintenance()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftMaintenance"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/05/2022.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftMaintenance(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

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
        /// Gets or sets the planned at airport - or NULL for next available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Airport PlannedAtAirport
        {
            get => this.LazyLoader.Load(this, ref this.plannedAtAirport);
            set => this.plannedAtAirport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets an optional airport ICAO code the maintenance should be performed at (NULL for
        /// next airport with required facilities)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 5)]
        public string PlannedAtICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the date/time the maintenance is planned for (NULL for immediately once aircraft is IDLE)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? PlannedFor { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft maintenance record number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int RecordNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the required manager hours to complete the maintenance (added to start date/time to calculate completed time).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int RequiredManHours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the maintenance was started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Started { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of technicians assigned (divides the man hours).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Technicians { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}