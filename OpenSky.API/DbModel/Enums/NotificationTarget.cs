// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationTarget.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification targets.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum NotificationTarget
    {
        /// <summary>
        /// Send notification to client.
        /// </summary>
        Client = 0,

        /// <summary>
        /// Send notification to agent.
        /// </summary>
        Agent = 1,

        /// <summary>
        /// Send notification to client and agent.
        /// </summary>
        ClientAndAgent = 2,

        /// <summary>
        /// Send notification via email.
        /// </summary>
        Email = 3,

        /// <summary>
        /// Send notification to ALL (including email).
        /// </summary>
        All = 4
    }
}