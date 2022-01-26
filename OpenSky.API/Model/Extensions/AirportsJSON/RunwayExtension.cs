// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunwayExtension.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Extensions.AirportsJSON
{
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Runway extension to quickly create JSON package runway.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class RunwayExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Runway extension method that constructs an airports JSON runway.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/12/2021.
        /// </remarks>
        /// <param name="runway">
        /// The runway to act on.
        /// </param>
        /// <returns>
        /// An OpenSky.AirportsJSON.Runway.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSky.AirportsJSON.Runway ConstructAirportsJson(this Runway runway)
        {
            return new OpenSky.AirportsJSON.Runway
            {
                Length = runway.Length,
                HasLighting = !string.IsNullOrEmpty(runway.CenterLight) || !string.IsNullOrEmpty(runway.EdgeLight)
            };
        }
    }
}