// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Payload.cs" company="OpenSky">
// OpenSky project 2021
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
    /// Payload model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Payload
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft the payload is currently loaded on, or NULL if stored at an airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Aircraft aircraft;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport the payload is currently stored at, or NULL if onboard an aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The destination airport for the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport destination;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Payload()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Payload(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft the payload is currently loaded on, or NULL if stored at an airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public Aircraft Aircraft
        {
            get => this.LazyLoader.Load(this, ref this.aircraft);
            set => this.aircraft = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry the payload is currently loaded on, or NULL if stored at an airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Aircraft")]
        [StringLength(10, MinimumLength = 5)]
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport the payload is currently stored at, or NULL if onboard an aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AirportICAO")]
        [JsonIgnore]
        public Airport Airport
        {
            get => this.LazyLoader.Load(this, ref this.airport);
            set => this.airport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport icao the payload is currently stored at, or NULL if onboard an aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Airport")]
        [StringLength(5, MinimumLength = 3)]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload description.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Description { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Destination for the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("DestinationICAO")]
        [JsonIgnore]
        public Airport Destination
        {
            get => this.LazyLoader.Load(this, ref this.destination);
            set => this.destination = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination icao for the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Destination")]
        [StringLength(5, MinimumLength = 3)]
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("JobID")]
        [JsonIgnore]
        public Job Job { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [ForeignKey("Job")]
        public Guid JobID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload weight in lbs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}