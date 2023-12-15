// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddNotification.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Notification
{
    using System;
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
        /// Optional: The number of seconds after which to auto-dismiss the notification (or NULL for no
        /// timeout)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int? DisplayTimeout { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Optional: The date/time after which the message gets sent via email if no client picked it up.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? EmailFallback { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the notification expires.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Expires { get; set; }

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