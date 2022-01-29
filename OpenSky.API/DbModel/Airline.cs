// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Airline.cs" company="OpenSky">
// OpenSky project 2021-2022
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
    /// Airline model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Airline
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The financial records for this airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<FinancialRecord> financialRecords;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flights of this airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> flights;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The members of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<OpenSkyUser> members;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline share holders.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AirlineShareHolder> shareHolders;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user permissions for this airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AirlineUserPermission> userPermissions;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Airline"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Airline()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Airline"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/10/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Airline(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the account balance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long AccountBalance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Country Country { get; set; }

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
        /// Gets or sets the flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<Flight> Flights
        {
            get => this.LazyLoader.Load(this, ref this.flights);
            set => this.flights = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user that founded this airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Founder")]
        [StringLength(255)]
        [Required]
        public string FounderID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the founding date of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime FoundingDate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the optional IATA code of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(2, MinimumLength = 2)]
        public string IATA { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string ICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<OpenSkyUser> Members
        {
            get => this.LazyLoader.Load(this, ref this.members);
            set => this.members = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline share holders.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AirlineShareHolder> ShareHolders
        {
            get => this.LazyLoader.Load(this, ref this.shareHolders);
            set => this.shareHolders = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user permissions for this airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AirlineUserPermission> UserPermissions
        {
            get => this.LazyLoader.Load(this, ref this.userPermissions);
            set => this.userPermissions = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}