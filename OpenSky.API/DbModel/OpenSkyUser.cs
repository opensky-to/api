﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyUser.cs" company="OpenSky">
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
        /// The airline roles for this user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AirlineUserRole> roles;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The access tokens of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<OpenSkyToken> tokens;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user operated flights (not airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Flight> flights;

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
        /// Gets or sets the user operated flights (not airline).
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
        /// Gets or sets the airline rank (or NULL if not member of an airline).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PilotRank? AirlineRank { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline salary (SkyBucks per NM flown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? AirlineSalary { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Bing maps API key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string BingMapsKey { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the last login.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LastLogin { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login geo location (country).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastLoginGeo { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login IP.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastLoginIP { get; set; }

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
        /// Gets or sets the user's airline roles.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AirlineUserRole> Roles
        {
            get => this.LazyLoader.Load(this, ref this.roles);
            set => this.roles = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Simbrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SimbriefUsername { get; set; }

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
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}