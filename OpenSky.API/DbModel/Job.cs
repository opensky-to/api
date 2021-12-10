// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Job.cs" company="OpenSky">
// OpenSky project 2021
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
    /// Job model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Job
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
        /// The assigned airline dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser assignedAirlineDispatcher;

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
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Job()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Job(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Job ValidEmptyModel =>
            new()
            {
                ID = Guid.Empty
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the assigned airline dispatcher.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AssignedAirlineDispatcherID")]
        [JsonIgnore]
        public OpenSkyUser AssignedAirlineDispatcher
        {
            get => this.LazyLoader.Load(this, ref this.assignedAirlineDispatcher);
            set => this.assignedAirlineDispatcher = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the assigned airline dispatcher (should not be set for user operated flights - will be ignored).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AssignedAirlineDispatcher")]
        [StringLength(255)]
        public string AssignedAirlineDispatcherID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the category of aircraft this job is intended for.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeCategory Category { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time the job expires at.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime ExpiresAt { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        public Guid ID { get; set; }

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
        public string OperatorAirlineID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the operator of this flight (either this or OperatorAirlineID
        /// must be set.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Operator")]
        [StringLength(255)]
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
        [JsonIgnore]
        public Airport Origin
        {
            get => this.LazyLoader.Load(this, ref this.origin);
            set => this.origin = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport icao.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Origin")]
        [StringLength(5, MinimumLength = 3)]
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payloads.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<Payload> Payloads { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public JobType Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a user-settable identifier string for the job.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        public string UserIdentifier { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the job value in SkyBucks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Value { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}