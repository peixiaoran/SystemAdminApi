using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity
{
    /// <summary>
    /// Passkey 绑定链接令牌实体类
    /// </summary>
    [SugarTable("[Basic].[PasskeyBindToken]")]
    public class PasskeyBindTokenEntity
    {
        /// <summary>
        /// 主键Id（雪花Id）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long BindTokenId { get; set; }

        /// <summary>
        /// 被绑定的用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 令牌哈希值（SHA256），不存明文
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// 链接发送到的邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 使用时间，为空表示未使用
        /// </summary>
        public DateTime? UsedDate { get; set; }

        /// <summary>
        /// 使用来源IP
        /// </summary>
        public string? UsedByIp { get; set; }

        /// <summary>
        /// 撤销时间（被新链接替换或发送失败），为空表示未撤销
        /// </summary>
        public DateTime? RevokedDate { get; set; }

        /// <summary>
        /// 签发人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 签发时间
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
