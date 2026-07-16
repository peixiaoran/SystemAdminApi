using Microsoft.AspNetCore.Http;

/// <summary>
/// JWT 配置选项
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 签名算法（当前使用 ES256）
    /// </summary>
    public string Algorithm { get; set; } = "ES256";

    /// <summary>
    /// Token 签发者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token 接收方
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token 有效期（分钟）
    /// </summary>
    public int ExpiresInMinutes { get; set; } = 120;

    /// <summary>
    /// 时间偏移容忍值（秒），用于容忍服务器间时间误差
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 10;

    /// <summary>
    /// 密钥 Id（kid），用于密钥轮换
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// 验签公钥（PEM 格式）
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// 签名私钥（PEM 格式，必须严格保密）
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// 存储 JWT 的 Cookie 名称
    /// </summary>
    public string? CookieName { get; set; } = "AccessToken";

    /// <summary>
    /// 是否仅 HTTPS 下发送 Cookie（生产环境建议 true）
    /// </summary>
    public bool CookieSecure { get; set; } = true;

    /// <summary>
    /// Cookie 的 SameSite 策略（前后端分离需 None）
    /// </summary>
    public SameSiteMode CookieSameSite { get; set; } = SameSiteMode.None;
}
