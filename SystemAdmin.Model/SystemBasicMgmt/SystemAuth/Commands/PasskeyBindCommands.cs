using Fido2NetLib;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands
{
    /// <summary>
    /// 凭绑定链接令牌查询请求参数
    /// </summary>
    public class PasskeyBindQuery
    {
        /// <summary>
        /// 邮件链接中的令牌
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// 凭绑定链接令牌完成 Passkey 绑定请求参数
    /// </summary>
    public class PasskeyBindComplete
    {
        /// <summary>
        /// 邮件链接中的令牌
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 设备名称（可为空）
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// navigator.credentials.create 返回的认证器响应
        /// </summary>
        public AuthenticatorAttestationRawResponse Attestation { get; set; } = default!;
    }
}
