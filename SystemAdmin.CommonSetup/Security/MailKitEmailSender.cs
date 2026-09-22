using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SqlSugar;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>MailKit 邮件发送</summary>
    public class MailKitEmailSender
    {
        private readonly EmailOptions _options;
        private readonly SqlSugarScope _db;

        public MailKitEmailSender(IOptions<EmailOptions> options, SqlSugarScope db)
        {
            _options = options.Value;
            _db = db;
        }

        /// <summary>通过 SMTP（587 / StartTls）发送邮件，无论成败均落库记录</summary>
        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                var mimeMessage = BuildMimeMessage(message);

                using var client = new SmtpClient { Timeout = _options.Timeout };

                await client.ConnectAsync(_options.SmtpServer, 587, SecureSocketOptions.StartTls, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_options.UserName))
                    await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);

                await client.SendAsync(mimeMessage, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                await LogSendResult(message, isSuccess: true, errorMessage: null);
            }
            catch (Exception ex)
            {
                await LogSendResult(message, isSuccess: false, ex.Message);
                throw;
            }
        }

        /// <summary>写入发送日志；日志本身写入失败不影响邮件发送结果的抛出</summary>
        private async Task LogSendResult(EmailMessage message, bool isSuccess, string? errorMessage)
        {
            try
            {
                var log = new EmailSendLogEntity
                {
                    ToAddress = string.Join(";", message.To),
                    CcAddress = message.Cc.Count > 0 ? string.Join(";", message.Cc) : null,
                    BccAddress = message.Bcc.Count > 0 ? string.Join(";", message.Bcc) : null,
                    Subject = message.Subject,
                    Body = message.Body,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    SendDate = DateTime.Now,
                };

                await _db.Insertable(log).ExecuteCommandAsync();
            }
            catch
            {
                // 忽略日志写入异常，不掩盖邮件发送本身的成败
            }
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
