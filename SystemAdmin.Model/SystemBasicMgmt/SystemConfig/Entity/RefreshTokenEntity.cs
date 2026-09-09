using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity
{
    /// <summary>
    /// Refresh Token 实体类
    /// </summary>
    [SugarTable("[Basic].[RefreshToken]")]
    public class RefreshTokenEntity
    {
        /// <summary>
        /// 主键Id（雪花Id）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long RefreshId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Token 哈希值（SHA256），不存明文
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 创建来源IP
        /// </summary>
        public string CreatedByIp { get; set; } = string.Empty;

        /// <summary>
        /// 撤销时间，为空表示仍有效
        /// </summary>
        public DateTime? RevokedDate { get; set; }

        /// <summary>
        /// 轮换后新 Token 的Id
        /// </summary>
        public long? ReplacedByTokenId { get; set; }
    }
}
