using Microsoft.AspNetCore.Http;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 请求语言访问器：从请求头 Accept-Language 解析当前语言
    /// </summary>
    public class LanguageAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前请求的语言（默认 zh-CN）
        /// </summary>
        public Language GetRequestLanguage()
        {
            var header = _httpContextAccessor.HttpContext?
                .Request.Headers["Accept-Language"].ToString();

            // 格式： "zh-CN,zh;q=0.9,en-US;q=0.8"
            var ui = "zh-CN"; // 默认语言

            if (!string.IsNullOrWhiteSpace(header))
            {
                var first = header.Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(first))
                    ui = first;
            }

            return new Language(ui);
        }
    }
}
