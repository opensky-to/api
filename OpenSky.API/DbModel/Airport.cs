﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Airport.cs" company="OpenSky">
// Flusinerd for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /*
     * AIRPORT EXAMPLE RECORD FROM DB (LOWW - Vienna International)
     *
     * INSERT INTO `Airports` (`ICAO`, `Altitude`, `AtisFrequency`, `City`, `GaRamps`, `Gates`, `HasAvGas`, `HasJetFuel`, `IsClosed`, `IsMilitary`, `Latitude`,
     * `LongestRunwayLength`, `LongestRunwaySurface`, `Longitude`, `Name`, `RunwayCount`, `TowerFrequency`, `UnicomFrequency`, `Size`, `MSFS`, `SupportsSuper`)
     *
     * VALUES ('LOWW', '0', '121730', 'Schwechat', '29', '31', '1', '1', '0', '0', '48.11007308959961',
     * '11811', 'A', '16.569616317749023', 'Flughafen Wien-Schwechat', '2', '119400', '118525', '5', '1', '1')
     */

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Airport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The approaches.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Approach> approaches;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Runway> runways;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Airport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Airport()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Airport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Airport(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the altitude of the airport in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the approaches.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<Approach> Approaches
        {
            get => this.LazyLoader.Load(this, ref this.approaches);
            set => this.approaches = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ATIS frequency (if available).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? AtisFrequency { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public Countries Country { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of GA ramps.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int GaRamps { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of gates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Gates { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport has AV gas for refueling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasAvGas { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport has jet fuel for refueling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasJetFuel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ICAO identifier of the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [StringLength(5, MinimumLength = 3)]
        [Required]
        public string ICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport is closed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsClosed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport is a military one.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsMilitary { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latitude of the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the length of the longest runway in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int LongestRunwayLength { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the surface type of the longest runway (can be "UNKNOWN" in a few cases).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(7)]
        [Required]
        public string LongestRunwaySurface { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the longitude of the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport if available in MSFS 2020.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool MSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int RunwayCount { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<Runway> Runways
        {
            get => this.LazyLoader.Load(this, ref this.runways);
            set => this.runways = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the size of the airport (from -1 to 6).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? Size { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport supports super-heavy aircraft like the Airbus A380.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SupportsSuper { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tower frequency (if available).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? TowerFrequency { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the unicom frequency (if available).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? UnicomFrequency { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}