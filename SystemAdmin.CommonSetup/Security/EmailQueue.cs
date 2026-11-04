using SqlSugar;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>邮件待发送队列：只写入 EmailOutbox 表，发送由独立的邮件服务完成</summary>
    public class EmailQueue
    {
        private readonly SqlSugarScope _db;

        public EmailQueue(SqlSugarScope db)
        {
            _db = db;
        }

        /// <summary>写入一封待发送邮件，在当前事务内执行</summary>
        public Task EnqueueAsync(EmailMessage message)
        {
            return EnqueueAsync(new[] { message });
        }

        /// <summary>批量写入待发送邮件，在当前事务内执行</summary>
        public async Task EnqueueAsync(IEnumerable<EmailMessage> messages)
        {
            var now = DateTime.Now;
            var rows = messages.Select(message => new EmailOutboxEntity
            {
                EmailId = SnowFlakeSingle.Instance.NextId(),
                ToAddress = string.Join(";", message.To),
                CcAddress = message.Cc.Count > 0 ? string.Join(";", message.Cc) : null,
                BccAddress = message.Bcc.Count > 0 ? string.Join(";", message.Bcc) : null,
                Subject = message.Subject,
                Body = message.Body,
                IsHtml = message.IsHtml,
                Attachments = message.Attachments.Count > 0 ? string.Join(";", message.Attachments) : null,
                CreatedDate = now,
            }).ToList();

            if (rows.Count > 0)
            {
                await _db.Insertable(rows).ExecuteCommandAsync();
            }
        }
    }
}
