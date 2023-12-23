﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupedNotificationRecipient.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Notification
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Grouped notification recipient.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class GroupedNotificationRecipient
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the agent has picked up the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AgentPickup { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the client has picked up the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ClientPickup { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the email was sent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EmailSent { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }
    }
}