namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>Cloudflare Turnstile 配置</summary>
    public class TurnstileOptions
    {
        /// <summary>Secret Key</summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>校验接口地址</summary>
        public string VerifyUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    }
}
