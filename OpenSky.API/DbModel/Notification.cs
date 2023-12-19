// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notification.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Notification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The recipient.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser recipient;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Notification()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/12/2023.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Notification(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Has the agent picked up the notification yet?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AgentPickup { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Has the client picked up the notification yet?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ClientPickup { get; set; }

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
        /// Has the email been sent?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EmailSent { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the notification expires.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Expires { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the shared group identifier for notifications created together.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Guid GroupingID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
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
        /// Gets or sets the recipient.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("RecipientID")]
        [JsonIgnore]
        public OpenSkyUser Recipient
        {
            get => this.LazyLoader.Load(this, ref this.recipient);
            set => this.recipient = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user who is the recipient of the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Recipient")]
        [StringLength(255)]
        [ConcurrencyCheck]
        public string RecipientID { get; set; }

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the target for the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public NotificationTarget Target { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}