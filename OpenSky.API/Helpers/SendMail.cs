// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendMail.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Helpers
{
    using JetBrains.Annotations;

    using MailKit.Net.Smtp;
    using MailKit.Security;

    using MimeKit;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Send mail helper.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class SendMail
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SendMail"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="smtpServer">
        /// The SMTP server.
        /// </param>
        /// <param name="smtpPort">
        /// The SMTP port.
        /// </param>
        /// <param name="userName">
        /// Optional: The name of the user.
        /// </param>
        /// <param name="password">
        /// Optional: The password.
        /// </param>
        /// <param name="secureSocketOptions">
        /// (Optional) Gets or sets options for controlling the secure socket.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SendMail(
            [System.Diagnostics.CodeAnalysis.NotNull]
            string smtpServer,
            int smtpPort,
            [CanBeNull] string userName,
            [CanBeNull] string password,
            SecureSocketOptions secureSocketOptions = SecureSocketOptions.Auto)
        {
            this.SmtpServer = smtpServer;
            this.SmtpPort = smtpPort;
            this.UserName = userName;
            this.Password = password;
            this.SecureSocketOptions = secureSocketOptions;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public string Password { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets options for controlling the secure socket.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SecureSocketOptions SecureSocketOptions { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int SmtpPort { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the SMTP server.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string SmtpServer { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public string UserName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Send email using SMTP(S).
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="from">
        /// The from address.
        /// </param>
        /// <param name="recipients">
        /// The recipients of the message (Comma separated).
        /// </param>
        /// <param name="recipientsCC">
        /// The recipients CC (Comma separated). This may be null.
        /// </param>
        /// <param name="recipientsBCC">
        /// The recipients BCC (Comma separated). This may be null.
        /// </param>
        /// <param name="subject">
        /// The subject of the message.
        /// </param>
        /// <param name="body">
        /// The body of the message.
        /// </param>
        /// <param name="isBodyHtml">
        /// Is the email body HTML formatted?.
        /// </param>
        /// <param name="priority">
        /// The priority of the message.
        /// </param>
        /// <param name="attachments">
        /// Array of email attachments to send.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SendEmail(
            [NotNull] string from,
            [NotNull] string recipients,
            [CanBeNull] string recipientsCC,
            [CanBeNull] string recipientsBCC,
            [NotNull] string subject,
            [CanBeNull] string body,
            bool isBodyHtml,
            MessagePriority priority,
            [CanBeNull] [ItemNotNull] params MimeEntity[] attachments)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            foreach (var to in recipients.Split(','))
            {
                message.To.Add(MailboxAddress.Parse(to));
            }

            if (!string.IsNullOrWhiteSpace(recipientsCC))
            {
                foreach (var cc in recipientsCC.Split(','))
                {
                    message.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            if (!string.IsNullOrWhiteSpace(recipientsBCC))
            {
                foreach (var bcc in recipientsBCC.Split(','))
                {
                    message.Bcc.Add(MailboxAddress.Parse(bcc));
                }
            }

            message.Subject = subject;
            message.Priority = priority;

            var bodyBuilder = new BodyBuilder();
            if (isBodyHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    bodyBuilder.Attachments.Add(attachment);
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Connect(this.SmtpServer, this.SmtpPort, this.SecureSocketOptions);
            if (!string.IsNullOrWhiteSpace(this.UserName) && this.Password?.Length > 0)
            {
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(this.UserName, this.Password);
            }

            client.Send(message);
            client.Disconnect(true);
        }
    }
}