// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightNavlogFix.cs" company="OpenSky">
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
    /// Flight navigation log fix model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 04/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightNavlogFix
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Flight flight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightNavlogFix"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightNavlogFix()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightNavlogFix"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/11/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FlightNavlogFix(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fix number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FixNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("FlightID")]
        public Flight Flight
        {
            get => this.LazyLoader.Load(this, ref this.flight);
            set => this.flight = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [ForeignKey("Flight")]
        public Guid FlightID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ident of the fix.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string Ident { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the fix.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}