// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PilotRank.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airline pilot ranks.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum PilotRank
    {
        /// <summary>
        /// Cadet.
        /// </summary>
        Cadet = 0,

        /// <summary>
        /// Second officer.
        /// </summary>  
        SecondOfficer = 1,

        /// <summary>
        /// First officer.
        /// </summary>
        FirstOfficer = 2,

        /// <summary>
        /// Captain.
        /// </summary>
        Captain = 3,

        /// <summary>
        /// Training captain.
        /// </summary>
        TrainingCaptain = 4
    }
}