using Fido2NetLib;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands
{
    /// <summary>
    /// 完成 Passkey 注册请求参数
    /// </summary>
    public class PasskeyRegisterComplete
    {
        /// <summary>
        /// 设备名称（用户自定义，可为空）
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// navigator.credentials.create 返回的认证器响应
        /// </summary>
        public AuthenticatorAttestationRawResponse Attestation { get; set; } = default!;
    }

    /// <summary>
    /// 完成 Passkey 登录请求参数
    /// </summary>
    public class PasskeyLoginComplete
    {
        /// <summary>
        /// 登录会话Id（由 PasskeyLoginOptions 返回）
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// navigator.credentials.get 返回的认证器响应
        /// </summary>
        public AuthenticatorAssertionRawResponse Assertion { get; set; } = default!;
    }

    /// <summary>
    /// 重命名 Passkey 请求参数
    /// </summary>
    public class PasskeyRename
    {
        /// <summary>
        /// Passkey Id
        /// </summary>
        public string PasskeyId { get; set; } = string.Empty;

        /// <summary>
        /// 新设备名称
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 删除 Passkey 请求参数
    /// </summary>
    public class PasskeyDelete
    {
        /// <summary>
        /// Passkey Id
        /// </summary>
        public string PasskeyId { get; set; } = string.Empty;
    }
}
