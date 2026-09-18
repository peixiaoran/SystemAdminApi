using Microsoft.AspNetCore.Http;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>JWT 配置（签名算法固定为 ES256）</summary>
    public class JwtSettings
    {
        /// <summary>签发者</summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>接收方</summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>Access Token 有效期（分钟）</summary>
        public int ExpiresInMinutes { get; set; } = 120;

        /// <summary>时钟偏移容忍（秒）</summary>
        public int ClockSkewSeconds { get; set; } = 10;

        /// <summary>密钥 Id（kid），用于密钥轮换</summary>
        public string KeyId { get; set; } = string.Empty;

        /// <summary>验签公钥（PEM）</summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>签名私钥（PEM，须保密）</summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>Access Token 的 Cookie 名称</summary>
        public string CookieName { get; set; } = "AccessToken";

        /// <summary>Refresh Token 有效期（天）</summary>
        public int RefreshTokenExpiresInDays { get; set; } = 30;

        /// <summary>Refresh Token 的 Cookie 名称</summary>
        public string RefreshCookieName { get; set; } = "RefreshToken";

        /// <summary>是否仅 HTTPS 下发送 Cookie</summary>
        public bool CookieSecure { get; set; } = true;

        /// <summary>Cookie 的 SameSite 策略（前后端分离需 None）</summary>
        public SameSiteMode CookieSameSite { get; set; } = SameSiteMode.None;
    }
}
