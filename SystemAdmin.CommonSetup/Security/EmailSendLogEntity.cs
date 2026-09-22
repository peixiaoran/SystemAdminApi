using SqlSugar;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 邮件发送日志实体类
    /// </summary>
    [SugarTable("[Basic].[EmailSendLog]")]
    public class EmailSendLogEntity
    {
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
        /// 是否发送成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendDate { get; set; }
    }
}
