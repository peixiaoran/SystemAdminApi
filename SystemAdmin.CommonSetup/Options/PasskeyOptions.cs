namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>Passkey（WebAuthn）配置</summary>
    public class PasskeyOptions
    {
        /// <summary>RP ID：前端页面的有效域名（不含协议与端口），正式环境如 18test.esonlife.com.cn，开发环境为 localhost</summary>
        public string RpId { get; set; } = string.Empty;

        /// <summary>RP 显示名称</summary>
        public string RpName { get; set; } = "System Admin";

        /// <summary>允许发起 WebAuthn 的前端 Origin（含协议与端口），如 https://18test.esonlife.com.cn、http://localhost:3001</summary>
        public string[] Origins { get; set; } = Array.Empty<string>();

        /// <summary>challenge 缓存有效期（秒）</summary>
        public int ChallengeExpiresInSeconds { get; set; } = 120;

        /// <summary>绑定链接有效期（分钟），默认 24 小时</summary>
        public int BindLinkExpiresInMinutes { get; set; } = 1440;

        /// <summary>同一用户两次发送绑定链接的最短间隔（秒）</summary>
        public int BindLinkMinIntervalSeconds { get; set; } = 60;
    }
}
