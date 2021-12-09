// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPayload.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight payload model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightPayload
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Flight flight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Payload payload;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPayload"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightPayload()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPayload"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FlightPayload(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Flight")]
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
        [ForeignKey("Flight")]
        public Guid FlightID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("PayloadID")]
        public Payload Payload
        {
            get => this.LazyLoader.Load(this, ref this.payload);
            set => this.payload = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Payload")]
        public Guid PayloadID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}