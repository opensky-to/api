// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddNotification.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Notification
{
    using System.ComponentModel.DataAnnotations;

    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add notification model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AddNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Optional: Gets or sets the number of seconds after which to auto-dismiss the notification (or NULL for no
        /// timeout)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? DisplayTimeout { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Optional: Gets or sets the number of hours after which the message gets sent via email if no client picked it up.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? EmailFallbackHours { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Optional: Gets or sets the expiration in minutes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? ExpiresInMinutes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(500)]
        [Required]
        public string Message { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the recipient type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationRecipient RecipientType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the recipient username (only if recipient type is user).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(256)]
        public string RecipientUserName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to send the notification as "OpenSky System"
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SendAsSystem { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the style of the notification (not used for email).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationStyle Style { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the target for the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationTarget Target { get; set; }
    }
}