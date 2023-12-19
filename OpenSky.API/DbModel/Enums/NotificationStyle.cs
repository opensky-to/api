// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationStyle.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification styles.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum NotificationStyle
    {
        /// <summary>
        /// Notification toast (default colors).
        /// </summary>
        ToastInfo = 0,

        /// <summary>
        /// Message box (default colors).
        /// </summary>
        MessageBoxInfo = 1,

        /// <summary>
        /// Notification toast (warning colors).
        /// </summary>
        ToastWarning = 2,

        /// <summary>
        /// Message box (warning colors).
        /// </summary>
        MessageBoxWarning = 3,

        /// <summary>
        /// Notification toast (error colors).
        /// </summary>
        ToastError = 4,

        /// <summary>
        /// Message box (error colors).
        /// </summary>
        MessageBoxError = 5
    }
}