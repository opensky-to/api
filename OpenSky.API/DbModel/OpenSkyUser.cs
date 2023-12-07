// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyUser.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using Microsoft.AspNetCore.Identity;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky user model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Identity.IdentityUser"/>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyUser : IdentityUser
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline the user belongs to (or NULL if not member of an airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline airline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline dispatcher assignments.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Job> airlineDispatcherAssignments;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline permissions for this user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AirlineUserPermission> airlinePermissions;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline pilot assignments.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> airlinePilotAssignments;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The dispatches (flight plans).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> dispatches;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The financial records for this user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<FinancialRecord> financialRecords;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user operated flights (not airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> flights;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user operated jobs (not airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Job> jobs;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline share holdings for this user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AirlineShareHolder> shareHoldings;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The access tokens of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<OpenSkyToken> tokens;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyUser"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyUser()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyUser"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyUser(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline the user belongs to (or NULL if not member of an airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("AirlineICAO")]
        public Airline Airline
        {
            get => this.LazyLoader.Load(this, ref this.airline);
            set => this.airline = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline dispatcher assignments.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [InverseProperty("AssignedAirlineDispatcher")]
        public ICollection<Job> AirlineDispatcherAssignments
        {
            get => this.LazyLoader.Load(this, ref this.airlineDispatcherAssignments);
            set => this.airlineDispatcherAssignments = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline ICAO code (or NULL if not member of an airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Airline")]
        [StringLength(3, MinimumLength = 3)]
        public string AirlineICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline income share (in percent, 20=>20% of job income).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? AirlineIncomeShare { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline permissions for this user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AirlineUserPermission> AirlinePermissions
        {
            get => this.LazyLoader.Load(this, ref this.airlinePermissions);
            set => this.airlinePermissions = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline pilot assignments.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [InverseProperty("AssignedAirlinePilot")]
        public ICollection<Flight> AirlinePilotAssignments
        {
            get => this.LazyLoader.Load(this, ref this.airlinePilotAssignments);
            set => this.airlinePilotAssignments = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline rank (or NULL if not member of an airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PilotRank? AirlineRank { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline salary (SkyBucks per flight hour).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? AirlineSalary { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Bing maps API key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(200)]
        public string BingMapsKey { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the dispatches (flight plans).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [InverseProperty("Dispatcher")]
        public ICollection<Flight> Dispatches
        {
            get => this.LazyLoader.Load(this, ref this.dispatches);
            set => this.dispatches = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the financial records.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<FinancialRecord> FinancialRecords
        {
            get => this.LazyLoader.Load(this, ref this.financialRecords);
            set => this.financialRecords = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user operated flights (not airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [InverseProperty("Operator")]
        public ICollection<Flight> Flights
        {
            get => this.LazyLoader.Load(this, ref this.flights);
            set => this.flights = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user operated jobs (not airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [InverseProperty("Operator")]
        public ICollection<Job> Jobs
        {
            get => this.LazyLoader.Load(this, ref this.jobs);
            set => this.jobs = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the last login.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LastLogin { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login geolocation (country).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(60)]
        public string LastLoginGeo { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login IP.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(30)]
        public string LastLoginIP { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the personal account balance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long PersonalAccountBalance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public byte[] ProfileImage { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the user registered.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime RegisteredOn { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user's airline share holdings.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AirlineShareHolder> ShareHoldings
        {
            get => this.LazyLoader.Load(this, ref this.shareHoldings);
            set => this.shareHoldings = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Simbrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(100)]
        public string SimbriefUsername { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the token renewal country verification is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool TokenRenewalCountryVerification { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the access tokens of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<OpenSkyToken> Tokens
        {
            get => this.LazyLoader.Load(this, ref this.tokens);
            set => this.tokens = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Vatsim user ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(10)]
        public string VatsimID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}