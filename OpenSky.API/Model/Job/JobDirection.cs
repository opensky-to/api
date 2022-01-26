// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobDirection.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Job
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Job directions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum JobDirection
    {
        /// <summary>
        /// Jobs FROM an airport.
        /// </summary>
        From = 0,

        /// <summary>
        /// Jobs TOWARDS an airport.
        /// </summary>
        To = 1,

        /// <summary>
        /// Jobs starting and ending at the same airport.
        /// </summary>
        RoundTrip = 2
    }
}