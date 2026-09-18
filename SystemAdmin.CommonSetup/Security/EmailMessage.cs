namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>待发送的邮件</summary>
    public class EmailMessage
    {
        /// <summary>收件人</summary>
        public List<string> To { get; set; } = new();

        /// <summary>抄送</summary>
        public List<string> Cc { get; set; } = new();

        /// <summary>密送</summary>
        public List<string> Bcc { get; set; } = new();

        /// <summary>主题</summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>正文</summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>正文是否为 HTML</summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>附件本地路径</summary>
        public List<string> Attachments { get; set; } = new();
    }
}
