using Fido2NetLib;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto
{
    /// <summary>
    /// Passkey 登录选项返回Dto
    /// </summary>
    public class PasskeyLoginOptionsDto
    {
        /// <summary>
        /// 登录会话Id，完成登录时需原样回传
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 传给 navigator.credentials.get 的 publicKey 参数
        /// </summary>
        public AssertionOptions Options { get; set; } = default!;
    }

    /// <summary>
    /// Passkey 列表Dto
    /// </summary>
    public class PasskeyDto
    {
        /// <summary>
        /// Passkey Id
        /// </summary>
        public string PasskeyId { get; set; } = string.Empty;

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
