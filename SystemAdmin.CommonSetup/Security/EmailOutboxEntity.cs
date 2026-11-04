using SqlSugar;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 待发送邮件实体类，由独立的邮件服务读取并发送
    /// </summary>
    [SugarTable("[Basic].[EmailOutbox]")]
    public class EmailOutboxEntity
    {
        /// <summary>
        /// 主键Id（雪花Id）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long EmailId { get; set; }

        /// <summary>
        /// 收件人（多个以 ; 分隔）
        /// </summary>
        public string ToAddress { get; set; } = string.Empty;

        /// <summary>
        /// 抄送（多个以 ; 分隔）
        /// </summary>
        public string? CcAddress { get; set; }

        /// <summary>
        /// 密送（多个以 ; 分隔）
        /// </summary>
        public string? BccAddress { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 正文
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// 正文是否为 HTML
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// 附件本地路径（多个以 ; 分隔）
        /// </summary>
        public string? Attachments { get; set; }

        /// <summary>
        /// 状态：0待发送 1发送中 2成功 3失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 已重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 下次重试时间，为空表示立即可发送
        /// </summary>
        public DateTime? NextRetryDate { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 发送成功时间
        /// </summary>
        public DateTime? SentDate { get; set; }
    }
}
