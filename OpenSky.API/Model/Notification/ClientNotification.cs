// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientNotification.cs" company="OpenSky">
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
    /// Client/Agent notification model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ClientNotification
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
        /// Gets or sets the identifier of the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid ID { get; set; }

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
        /// Gets or sets the sender (or NULL for OpenSky System).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(256)]
        public string Sender { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the style of the notification (not used for email).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationStyle Style { get; set; }
    }
}