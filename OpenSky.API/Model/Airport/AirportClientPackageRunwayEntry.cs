// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackageRunwayEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Airport
{
    using System.Collections.Generic;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport client package runway entries for airports.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackageRunwayEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportClientPackageRunwayEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/12/2021.
        /// </remarks>
        /// <param name="runway">
        ///     The runway record from the database.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirportClientPackageRunwayEntry(Runway runway)
        {
            this.Length = runway.Length;
            this.HasLighting = !string.IsNullOrEmpty(runway.CenterLight) || !string.IsNullOrEmpty(runway.EdgeLight);
            this.RunwayEnds = new List<AirportClientPackageRunwayEndEntry>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this object has lighting.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasLighting { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the length of the runway in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Length { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway ends.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<AirportClientPackageRunwayEndEntry> RunwayEnds { get; set; }
    }
}