using System.Globalization;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>当前请求的语言</summary>
    public class Language
    {
        private const string DefaultLocale = "zh-CN";

        /// <summary>语言标识，如 zh-CN</summary>
        public string Locale { get; }

        /// <summary>对应的 CultureInfo，无法识别时回退 zh-CN</summary>
        public CultureInfo Culture { get; }

        /// <summary>是否中文</summary>
        public bool IsChinese { get; }

        /// <summary>是否英文</summary>
        public bool IsEnglish { get; }

        public Language(string locale)
        {
            Locale = locale;
            Culture = ToCulture(locale) ?? new CultureInfo(DefaultLocale);
            IsChinese = locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            IsEnglish = locale.StartsWith("en", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>解析 Accept-Language 请求头，取第一个语言，缺省 zh-CN</summary>
        public static Language Parse(string? acceptLanguage)
        {
            var first = acceptLanguage?.Split(',')[0].Split(';')[0].Trim();
            return new Language(string.IsNullOrEmpty(first) ? DefaultLocale : first);
        }

        /// <summary>语言名称转 CultureInfo，无效时返回 null</summary>
        internal static CultureInfo? ToCulture(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                return new CultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}
