// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserAirline.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Airline
{
    using System.ComponentModel.DataAnnotations;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// User airline information data contract.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UserAirline
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAirline"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public UserAirline()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAirline"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/10/2021.
        /// </remarks>
        /// <param name="airline">
        /// The airline from the database.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public UserAirline(Airline airline)
        {
            this.Name = airline.Name;
            this.ICAO = airline.ICAO;
            this.IATA = airline.IATA;
        }

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
        [StringLength(3, MinimumLength = 3)]
        public string ICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50, MinimumLength = 4)]
        public string Name { get; set; }
    }
}