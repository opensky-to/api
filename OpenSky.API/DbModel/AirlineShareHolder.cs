﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlineShareHolder.cs" company="OpenSky">
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
    /// Airline share holders model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirlineShareHolder
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline airline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser user;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlineShareHolder"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AirlineShareHolder()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlineShareHolder"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/10/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirlineShareHolder(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline.
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
        /// Gets or sets the airline ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [ForeignKey("Airline")]
        public string AirlineICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the shares owned by the user (can be 0).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Shares { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public OpenSkyUser User
        {
            get => this.LazyLoader.Load(this, ref this.user);
            set => this.user = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(255)]
        [ForeignKey("Founder")]
        public string UserID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}