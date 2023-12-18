// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailWorkerService.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using MimeKit;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Email worker service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/12/2023.
    /// </remarks>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService"/>
    /// -------------------------------------------------------------------------------------------------
    public class EmailWorkerService : BackgroundService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The check for new email interval (1 minute).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const int CheckInterval = 1 * 60 * 1000;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<EmailWorkerService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The service provider.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IServiceProvider services;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="services">
        /// The service provider.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public EmailWorkerService(
            IServiceProvider services,
            ILogger<EmailWorkerService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Indicates that the shutdown process should no longer be graceful.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService.StopAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Email background service stopping...");
            return base.StopAsync(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <remarks>
        /// See <see href="https://docs.microsoft.com/dotnet/core/extensions/workers">Worker Services in
        /// .NET</see> for implementation guidelines.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService.ExecuteAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Email background service starting...");
            using var scope = this.services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var sendMail = scope.ServiceProvider.GetRequiredService<SendMail>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await this.SendNotificationEmails(db, configuration, sendMail);

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Send notification emails.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2023.
        /// </remarks>
        /// <param name="db">
        /// The database.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="sendMail">
        /// The send mail.
        /// </param>
        /// <returns>
        /// A Task.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task SendNotificationEmails(OpenSkyDbContext db, IConfiguration configuration, SendMail sendMail)
        {
            try
            {
                var notifications = await db.Notifications.Where(
                                                n =>
                                                    !n.EmailSent &&
                                                    (!n.Expires.HasValue || n.Expires.Value > DateTime.UtcNow) &&
                                                    (n.Target == NotificationTarget.Email || n.Target == NotificationTarget.All || (n.EmailFallback.HasValue && n.EmailFallback > DateTime.UtcNow)))
                                            .Include(notification => notification.Recipient)
                                            .ToListAsync();

                foreach (var notification in notifications)
                {
                    var emailBody = await System.IO.File.ReadAllTextAsync("EmailTemplates/Notification.html");
                    emailBody = emailBody.Replace("{UserName}", notification.RecipientID);
                    emailBody = emailBody.Replace("{LogoUrl}", configuration["OpenSky:LogoUrl"]);
                    emailBody = emailBody.Replace("{Sender}", notification.Sender);
                    emailBody = emailBody.Replace("{Message}", HttpUtility.HtmlEncode(notification.Message));
                    var backgroundColor = "#6e6e6e";
                    switch (notification.Style)
                    {
                        case NotificationStyle.ToastInfo:
                        case NotificationStyle.MessageBoxInfo:
                            backgroundColor = "#6e6e6e";
                            break;

                        case NotificationStyle.ToastWarning:
                        case NotificationStyle.MessageBoxWarning:
                            backgroundColor = "#ff8c00";
                            break;

                        case NotificationStyle.ToastError:
                        case NotificationStyle.MessageBoxError:
                            backgroundColor = "#8b0000";
                            break;
                    }

                    emailBody = emailBody.Replace("{BackgroundColor}", backgroundColor);

                    try
                    {
                        // ReSharper disable AssignNullToNotNullAttribute
                        sendMail.SendEmail(configuration["Email:FromAddress"], notification.Recipient?.Email, null, null, "OpenSky Notification", emailBody, true, MessagePriority.Normal);

                        // ReSharper restore AssignNullToNotNullAttribute
                        notification.EmailSent = true;
                        var saveEx = await db.SaveDatabaseChangesAsync(this.logger, "Error marking notification email sent");
                        if (saveEx != null)
                        {
                            throw saveEx;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Error sending notification {notification.ID} to user {notification.RecipientID}, with email address {notification.Recipient?.Email}.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error sending notification emails.");
            }
        }
    }
}