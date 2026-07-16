using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// MailKit 邮件发送服务
    /// </summary>
    public class MailKitEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public async Task SendAsync( EmailMessage message, CancellationToken cancellationToken = default)
        {
            var mimeMessage = BuildMimeMessage(message);

            using var client = new SmtpClient
            {
                Timeout = _options.Timeout
            };

            await client.ConnectAsync( _options.SmtpServer, 587, SecureSocketOptions.StartTls, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(
                    _options.UserName,
                    _options.Password,
                    cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);

            await client.DisconnectAsync(true, cancellationToken);
        }

        /// <summary>
        /// 构建 MimeMessage 邮件对象
        /// </summary>
        private MimeMessage BuildMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(
                _options.DisplayName,
                _options.From));

            foreach (var to in message.To)
                mimeMessage.To.Add(MailboxAddress.Parse(to));

            mimeMessage.Subject = message.Subject ?? string.Empty;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.IsHtml ? message.Body : null,
                TextBody = message.IsHtml ? null : message.Body
            };

            if (message.Attachments != null)
            {
                foreach (var path in message.Attachments)
                {
                    if (!string.IsNullOrWhiteSpace(path)
                        && File.Exists(path))
                    {
                        bodyBuilder.Attachments.Add(path);
                    }
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            return mimeMessage;
        }
    }
}
