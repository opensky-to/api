// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Airport.cs" company="OpenSky">
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

    using GeoCoordinatePortable;

    using Microsoft.EntityFrameworkCore;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /*
     * AIRPORT EXAMPLE RECORD FROM DB (LOWW - Vienna International)
     *
     * INSERT INTO `Airports` (`ICAO`, `Altitude`, `AtisFrequency`, `City`, `GaRamps`, `Gates`, `HasAvGas`, `HasJetFuel`, `IsClosed`, `IsMilitary`, `Latitude`,
     * `LongestRunwayLength`, `LongestRunwaySurface`, `Longitude`, `Name`, `RunwayCount`, `TowerFrequency`, `UnicomFrequency`, `Size`, `MSFS`, `SupportsSuper`,
     * `HasBeenPopulated`)
     *
     * VALUES ('LOWW', '0', '121730', 'Schwechat', '29', '31', '1', '1', '0', '0', '48.11007308959961',
     * '11811', 'A', '16.569616317749023', 'Flughafen Wien-Schwechat', '2', '119400', '118525', '5', '1', '1', '0')
     */

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [Index(nameof(S2Cell3))]
    [Index(nameof(S2Cell4))]
    [Index(nameof(S2Cell5))]
    [Index(nameof(S2Cell6))]
    [Index(nameof(S2Cell7))]
    [Index(nameof(S2Cell8))]
    [Index(nameof(S2Cell9))]
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
        /// The jobs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Job> jobs;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft types delivered here.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftManufacturerDeliveryLocation> deliveredHere;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payloads currently stored at this airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<Payload> payloads;

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
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Airport ValidEmptyModel =>
            new()
            {
                ICAO = "XXXX",
                LongestRunwaySurface = "XXXX",
                Name = "XXXX"
            };

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
        /// Gets or sets the AV gas price in SkyBucks/Gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public float AvGasPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        public string City { get; set; }

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
        /// Gets the geo coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [NotMapped]
        public GeoCoordinate GeoCoordinate => new(this.Latitude, this.Longitude, this.Altitude);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport has AV gas for refueling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasAvGas { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the HasBeenPopulated flag for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ProcessingStatus HasBeenPopulatedMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the HasBeenPopulated flag for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ProcessingStatus HasBeenPopulatedXP11 { get; set; }

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
        /// Gets or sets the jet fuel price in SkyBucks/Gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public float JetFuelPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the jobs that have this airport as their origin.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<Job> Jobs
        {
            get => this.LazyLoader.Load(this, ref this.jobs);
            set => this.jobs = value;
        }

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
        /// Gets or sets the aircraft types delivered here.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<AircraftManufacturerDeliveryLocation> DeliveredHere
        {
            get => this.LazyLoader.Load(this, ref this.deliveredHere);
            set => this.deliveredHere = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport is available in MSFS 2020.
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
        /// Gets or sets the payloads currently stored at this airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [InverseProperty("Airport")]
        [JsonIgnore]
        public ICollection<Payload> Payloads
        {
            get => this.LazyLoader.Load(this, ref this.payloads);
            set => this.payloads = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the previous size of the airport (if available, used to detect size changes and
        /// trigger other services like the aircraft world populator).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? PreviousSize { get; set; }

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
        public ICollection<Runway> Runways
        {
            get => this.LazyLoader.Load(this, ref this.runways);
            set => this.runways = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 3.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell3 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 4.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell4 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 5.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell5 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 6.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell6 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 7.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell7 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 8.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell8 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// S2 geometry cell ID for level 9.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ulong S2Cell9 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the size of the airport (from -1 to 6, NULL means size isn't calculated yet).
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
        /// Gets or sets a value indicating whether the airport is available in XPlane 11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool XP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}