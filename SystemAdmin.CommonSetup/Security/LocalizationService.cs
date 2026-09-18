using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;
using SystemAdmin.Localization.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>多语言消息服务：按 {ModulePath}.{ResxKey} 定位资源并返回文案</summary>
    public class LocalizationService
    {
        // 以任意一个强类型 Messages 类定位 Localization 程序集
        private static readonly Assembly LocalizationAssembly = typeof(Messages).Assembly;
        private static readonly ConcurrentDictionary<string, ResourceManager> ResourceManagers = new();
        private static readonly CultureInfo[] FallbackCultures = { new("zh-CN"), new("en-US") };

        private readonly Language _language;

        public LocalizationService(Language language)
        {
            _language = language;
        }

        /// <summary>按当前请求语言获取文案，缺失时返回 fullKey</summary>
        public string ReturnMsg(string fullKey, params object?[] args)
            => GetMessage(fullKey, _language.Culture, args);

        /// <summary>按指定语言获取文案，语言无效时回退请求语言</summary>
        public string ReturnMsg(string fullKey, string language, params object?[] args)
            => GetMessage(fullKey, Language.ToCulture(language) ?? _language.Culture, args);

        private static string GetMessage(string fullKey, CultureInfo culture, object?[] args)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
                return string.Empty;

            var (modulePath, resxKey) = SplitKey(fullKey);
            var resourceManager = GetResourceManager(modulePath);
            if (resourceManager is null)
                return fullKey;

            var value = resourceManager.GetString(resxKey, culture);
            foreach (var fallback in FallbackCultures)
            {
                if (!string.IsNullOrEmpty(value)) break;
                value = resourceManager.GetString(resxKey, fallback);
            }

            if (string.IsNullOrEmpty(value))
                return fullKey;

            if (args is not { Length: > 0 })
                return value;

            try
            {
                return string.Format(value, args);
            }
            catch (FormatException)
            {
                return value;
            }
        }

        /// <summary>拆分为模块路径与资源键；无模块路径时整体当作资源键</summary>
        private static (string ModulePath, string ResxKey) SplitKey(string fullKey)
        {
            var lastDot = fullKey.LastIndexOf('.');
            return lastDot <= 0 || lastDot == fullKey.Length - 1
                ? (string.Empty, fullKey)
                : (fullKey[..lastDot], fullKey[(lastDot + 1)..]);
        }

        private static ResourceManager? GetResourceManager(string modulePath)
        {
            if (string.IsNullOrWhiteSpace(modulePath))
                return null;

            return ResourceManagers.GetOrAdd(
                $"SystemAdmin.Localization.{modulePath}.Messages",
                baseName => new ResourceManager(baseName, LocalizationAssembly));
        }
    }
}
