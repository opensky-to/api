// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunwayEndExtension.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Extensions.AirportsJSON
{
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// RunwayEnd extension to quickly create JSON package runway end.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class RunwayEndExtension
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A RunwayEnd extension method that constructs an airports JSON runway end.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/12/2021.
        /// </remarks>
        /// <param name="runwayEnd">
        /// The runway end to act on.
        /// </param>
        /// <returns>
        /// An OpenSky.AirportsJSON.RunwayEnd.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSky.AirportsJSON.RunwayEnd ConstructAirportsJson(this RunwayEnd runwayEnd)
        {
            return new OpenSky.AirportsJSON.RunwayEnd
            {
                Name = runwayEnd.Name,
                Latitude = runwayEnd.Latitude,
                Longitude = runwayEnd.Longitude,
                HasClosedMarkings = runwayEnd.HasClosedMarkings
            };
        }
    }
}