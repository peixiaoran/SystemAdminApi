using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity
{
    /// <summary>
    /// 用户 Passkey 凭据实体类
    /// </summary>
    [SugarTable("[Basic].[UserPasskey]")]
    public class UserPasskeyEntity
    {
        /// <summary>
        /// 主键Id（雪花Id）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long PasskeyId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 凭据Id（Base64Url）
        /// </summary>
        public string CredentialId { get; set; } = string.Empty;

        /// <summary>
        /// 公钥（COSE 编码）
        /// </summary>
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// 签名计数器
        /// </summary>
        public long SignCount { get; set; }

        /// <summary>
        /// 认证器 AAGUID
        /// </summary>
        public Guid AaGuid { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public DateTime? LastUsedDate { get; set; }
    }
}
