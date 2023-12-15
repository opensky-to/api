// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationRecipient.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification recipients.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum NotificationRecipient
    {
        /// <summary>
        /// Send notification to single user.
        /// </summary>
        User = 0,

        /// <summary>
        /// Send notification to all mods and admins.
        /// </summary>
        Mods = 1,

        /// <summary>
        /// Send notification to all admins.
        /// </summary>
        Admins = 2,

        /// <summary>
        /// Send notification to EVERYONE.
        /// </summary>
        Everyone = 3
    }
}