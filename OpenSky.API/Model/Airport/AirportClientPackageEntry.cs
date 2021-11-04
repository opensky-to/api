﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackageEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Airport
{
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// One line of the airport client package json file.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackageEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportClientPackageEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport record from the database.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirportClientPackageEntry(Airport airport)
        {
            this.ICAO = airport.ICAO;
            this.Name = airport.Name;
            this.Latitude = airport.Latitude;
            this.Longitude = airport.Longitude;
            this.Size = airport.Size ?? -1;

            this.HasAvGas = airport.HasAvGas;
            this.HasJetFuel = airport.HasJetFuel;
            this.IsClosed = airport.IsClosed;
            this.IsMilitary = airport.IsMilitary;
            this.SupportsSuper = airport.SupportsSuper;
            this.MSFS = airport.MSFS;
        }

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
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the size of the airport (from -1 to 6).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Size { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the airport supports super-heavy aircraft like the Airbus A380.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SupportsSuper { get; set; }
    }
}