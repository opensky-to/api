// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirportClientPackageRoot.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Airport
{
    using System.Collections.Generic;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airport client package root json object.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirportClientPackageRoot
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirportClientPackageRoot"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/09/2021.
        /// </remarks>
        /// <param name="airports">
        /// Gets or sets the airports.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirportClientPackageRoot(List<AirportClientPackageEntry> airports)
        {
            this.Airports = airports;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<AirportClientPackageEntry> Airports { get; set; }
    }
}