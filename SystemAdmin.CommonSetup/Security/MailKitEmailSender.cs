using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>MailKit 邮件发送</summary>
    public class MailKitEmailSender
    {
        private readonly EmailOptions _options;

        public MailKitEmailSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>通过 SMTP（587 / StartTls）发送邮件</summary>
        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var mimeMessage = BuildMimeMessage(message);

            using var client = new SmtpClient { Timeout = _options.Timeout };

            await client.ConnectAsync(_options.SmtpServer, 587, SecureSocketOptions.StartTls, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
                await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        private MimeMessage BuildMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage
            {
                Subject = message.Subject ?? string.Empty
            };

            mimeMessage.From.Add(new MailboxAddress(_options.DisplayName, _options.From));

            foreach (var to in message.To)
                mimeMessage.To.Add(MailboxAddress.Parse(to));

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.IsHtml ? message.Body : null,
                TextBody = message.IsHtml ? null : message.Body
            };

            foreach (var path in message.Attachments ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    bodyBuilder.Attachments.Add(path);
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();
            return mimeMessage;
        }
    }
}
