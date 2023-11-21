// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingStatus.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// General purpose enum for handling different states of an entity Import, queued tasks etc...
    /// </summary>
    /// <remarks>
    /// Flusinerd, 25/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum ProcessingStatus
    {
        /// <summary>
        /// Needs handling by a worker.
        /// </summary>
        NeedsHandling = 0,

        /// <summary>
        /// Has been queued by a worker.
        /// </summary>
        Queued = 1,

        /// <summary>
        /// Has been finished and needs no more handling.
        /// </summary>
        Finished = 2,

        /// <summary>
        /// Last attempt has failed
        /// </summary>
        Failed = 3,
    }
}