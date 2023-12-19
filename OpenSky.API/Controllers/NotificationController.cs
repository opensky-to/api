// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationController.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Model.Notification;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<NotificationController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/12/2023.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// <param name="userManager">
        /// The user manager.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public NotificationController(ILogger<NotificationController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add a new notification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/12/2023.
        /// </remarks>
        /// <param name="addNotification">
        /// The add notification model.
        /// </param>
        /// <returns>
        /// An ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost(Name = "AddNotification")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> AddNotification([FromBody] AddNotification addNotification)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Notification");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var userRoles = await this.userManager.GetRolesAsync(user);

                if (!userRoles.Contains(UserRoles.Admin) && (addNotification.RecipientType == NotificationRecipient.Everyone || addNotification.Target is NotificationTarget.Email or NotificationTarget.All || addNotification.SendAsSystem))
                {
                    return new ApiResponse<string> { Message = "Unauthorized request!", IsError = true };
                }

                if (addNotification.DisplayTimeout is > 300)
                {
                    return new ApiResponse<string> { Message = "Maximum display timeout is 300 seconds!", IsError = true };
                }

                if (addNotification.ExpiresInMinutes is < 5)
                {
                    return new ApiResponse<string> { Message = "Notification expiration has to be at least 5 minutes!", IsError = true };
                }

                if (addNotification.EmailFallbackHours is < 1)
                {
                    return new ApiResponse<string> { Message = "Notification email fallback has to be at least 1 hour!", IsError = true };
                }

                if (string.IsNullOrEmpty(addNotification.Message))
                {
                    return new ApiResponse<string> { Message = "Notification message missing!", IsError = true };
                }

                if (addNotification.Message.Length > 500)
                {
                    return new ApiResponse<string> { Message = "Notification message exceeds 500 characters!", IsError = true };
                }

                if (addNotification.ExpiresInMinutes.HasValue && addNotification.EmailFallbackHours.HasValue && addNotification.ExpiresInMinutes.Value / 60.0 < addNotification.EmailFallbackHours.Value)
                {
                    return new ApiResponse<string> { Message = "Notification email fallback can't be past expiry!", IsError = true };
                }

                var recipients = new HashSet<string>();
                if (addNotification.RecipientType == NotificationRecipient.User)
                {
                    if (string.IsNullOrEmpty(addNotification.RecipientUserName))
                    {
                        return new ApiResponse<string> { Message = "Recipient user name missing!", IsError = true };
                    }

                    var recipientUser = await this.userManager.FindByNameAsync(addNotification.RecipientUserName);
                    if (recipientUser == null)
                    {
                        return new ApiResponse<string> { Message = $"Recipient user with name \"{addNotification.RecipientUserName}\" not found!", IsError = true };
                    }

                    recipients.Add(recipientUser.Id);
                }

                if (addNotification.RecipientType == NotificationRecipient.Mods)
                {
                    var mods = await this.userManager.GetUsersInRoleAsync(UserRoles.Moderator);
                    foreach (var mod in mods)
                    {
                        recipients.Add(mod.Id);
                    }
                }

                if (addNotification.RecipientType is NotificationRecipient.Mods or NotificationRecipient.Admins)
                {
                    var admins = await this.userManager.GetUsersInRoleAsync(UserRoles.Admin);
                    foreach (var admin in admins)
                    {
                        recipients.Add(admin.Id);
                    }
                }

                if (addNotification.RecipientType == NotificationRecipient.Everyone)
                {
                    var userIDs = await this.db.Users.Select(u => u.Id).ToListAsync();
                    foreach (var userID in userIDs)
                    {
                        recipients.Add(userID);
                    }
                }

                // All checks complete, let's create the notification records
                var groupingID = Guid.NewGuid();
                foreach (var recipient in recipients)
                {
                    var newNotification = new Notification
                    {
                        ID = Guid.NewGuid(),
                        GroupingID = groupingID,
                        RecipientID = recipient,
                        Message = addNotification.Message,
                        Sender = addNotification.SendAsSystem ? "OpenSky System" : user.UserName,
                        Target = addNotification.Target,
                        Expires = addNotification.ExpiresInMinutes.HasValue ? DateTime.UtcNow.AddMinutes(addNotification.ExpiresInMinutes.Value) : null,
                        EmailFallback = addNotification.EmailFallbackHours.HasValue ? DateTime.UtcNow.AddHours(addNotification.EmailFallbackHours.Value) : null,
                        Style = addNotification.Style,
                        DisplayTimeout = addNotification.DisplayTimeout
                    };

                    await this.db.Notifications.AddAsync(newNotification);
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new notifications.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Notification for {recipients.Count} recipient{(recipients.Count != 1 ? "s" : string.Empty)} added.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Notification");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Confirm successful pickup of a notification for the specified target.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="notificationID">
        /// Identifier for the notification.
        /// </param>
        /// <param name="target">
        /// The target of the notification.
        /// </param>
        /// <returns>
        /// An ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("{notificationID:guid}/{target}", Name = "ConfirmNotificationPickup")]
        public async Task<ActionResult<ApiResponse<string>>> ConfirmNotificationPickup(Guid notificationID, NotificationTarget target)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Notification/{notificationID}/{target}");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var notification = await this.db.Notifications.SingleOrDefaultAsync(n => n.ID == notificationID && n.RecipientID == user.Id);
                if (notification == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find specified notification!", IsError = true };
                }

                switch (target)
                {
                    case NotificationTarget.Client:
                        notification.ClientPickup = true;
                        break;
                    case NotificationTarget.Agent:
                        notification.AgentPickup = true;
                        break;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error marking notification as picked up.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>("Success");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Notification/{notificationID}/{target}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Delete specified notification group.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2023.
        /// </remarks>
        /// <param name="groupingID">
        /// Identifier for the grouping.
        /// </param>
        /// <returns>
        /// An ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpDelete("{groupingID:guid}", Name = "DeleteNotification")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteNotification(Guid groupingID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | DELETE Notification/{groupingID}");

                var notificationsToDelete = this.db.Notifications.Where(n => n.GroupingID == groupingID);
                this.db.Notifications.RemoveRange(notificationsToDelete);

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error deleting notifications.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>("Success");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | DELETE Notification/{groupingID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Active fallback to email NOW for specified notification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2023.
        /// </remarks>
        /// <param name="groupingID">
        /// Identifier for the grouping.
        /// </param>
        /// <returns>
        /// An ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("fallbackEmailNow/{groupingID:guid}", Name = "FallbackNotificationToEmailNow")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> FallbackNotificationToEmailNow(Guid groupingID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Notification/fallbackEmailNow/{groupingID}");

                var notificationsToFallback = await this.db.Notifications.Where(n => n.GroupingID == groupingID).ToListAsync();
                foreach (var notification in notificationsToFallback)
                {
                    notification.EmailFallback = DateTime.UtcNow;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error updating notifications.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>("Success");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT Notification/fallbackEmailNow/{groupingID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get all currently active notifications.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2023.
        /// </remarks>
        /// <returns>
        /// All notifications.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("all", Name = "GetAllNotifications")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<GroupedNotification>>>> GetAllNotifications()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Notification/all");

                var notifications = await this.db.Notifications.Include(notification => notification.Recipient).ToListAsync();
                var groupingIDs = notifications.Select(n => n.GroupingID).Distinct();
                var result = new List<GroupedNotification>();

                foreach (var groupingID in groupingIDs)
                {
                    var first = notifications.First(n => n.GroupingID == groupingID);
                    var groupedNotification = new GroupedNotification
                    {
                        Recipients = new List<GroupedNotificationRecipient>(),
                        GroupingID = first.GroupingID,
                        Sender = first.Sender,
                        Message = first.Message,
                        Target = first.Target,
                        Style = first.Style,
                        DisplayTimeout = first.DisplayTimeout,
                        Expires = first.Expires,
                        EmailFallback = first.EmailFallback,
                        MarkedForDeletion = first.MarkedForDeletion
                    };

                    groupedNotification.Recipients.AddRange(
                        notifications.Where(n => n.GroupingID == groupingID).Select(
                            n => new GroupedNotificationRecipient
                            {
                                ID = n.RecipientID,
                                Name = n.Recipient.UserName,
                                ClientPickup = n.ClientPickup,
                                AgentPickup = n.AgentPickup,
                                EmailSent = n.EmailSent
                            }));

                    result.Add(groupedNotification);
                }

                return new ApiResponse<IEnumerable<GroupedNotification>>(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Notification/all");
                return new ApiResponse<IEnumerable<GroupedNotification>>(ex) { Data = new List<GroupedNotification>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get notifications for specified target.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="target">
        /// The target for the notification.
        /// </param>
        /// <returns>
        /// The notifications for this target.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet(Name = "GetNotifications")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClientNotification>>>> GetNotifications(NotificationTarget target)
        {
            try
            {
                this.logger.LogTrace($"{this.User.Identity?.Name} | GET Notification");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<ClientNotification>> { Message = "Unable to find user record!", IsError = true, Data = new List<ClientNotification>() };
                }

                if (target is not (NotificationTarget.Client or NotificationTarget.Agent))
                {
                    return new ApiResponse<IEnumerable<ClientNotification>> { Message = "Can only pick up notifications for client and agent!", IsError = true, Data = new List<ClientNotification>() };
                }

                var notifications = await this.db.Notifications.Where(
                                                  n =>
                                                      n.RecipientID == user.Id &&
                                                      (!n.Expires.HasValue || n.Expires.Value > DateTime.UtcNow) &&
                                                      (n.Target == target || n.Target == NotificationTarget.ClientAndAgent || n.Target == NotificationTarget.All))
                                              .ToListAsync();

                switch (target)
                {
                    case NotificationTarget.Client:
                        notifications = notifications.Where(n => !n.ClientPickup).ToList();
                        break;
                    case NotificationTarget.Agent:
                        notifications = notifications.Where(n => !n.AgentPickup).ToList();
                        break;
                }

                return new ApiResponse<IEnumerable<ClientNotification>>(
                    notifications.Select(
                        n => new ClientNotification
                        {
                            ID = n.ID,
                            Sender = n.Sender,
                            Message = n.Message,
                            Style = n.Style,
                            DisplayTimeout = n.DisplayTimeout
                        }).ToList());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Notification");
                return new ApiResponse<IEnumerable<ClientNotification>>(ex) { Data = new List<ClientNotification>() };
            }
        }
    }
}