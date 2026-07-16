using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;
using SystemAdmin.Localization.SystemBasicMgmt.SystemAuth; // 用作定位 Localization 程序集锚点

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 多语言消息服务：按 fullKey（{ModulePath}.{ResxKey}）定位资源文件并返回对应语言的文案
    /// </summary>
    public class LocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Localization 资源所在程序集（锚点：任意一个 Messages 强类型类）
        private static readonly Assembly _localizationAssembly = typeof(Messages).Assembly;

        // ResourceManager 缓存：避免每次 new
        private static readonly ConcurrentDictionary<string, ResourceManager> _rmCache = new();

        public LocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取多语言文案
        /// </summary>
        /// <param name="fullKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string ReturnMsg(string fullKey, params object?[] args)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
                return string.Empty;

            // 解析：模块路径 + resxKey
            var (modulePath, resxKey) = ResolveModuleAndResxKey(fullKey);

            // 拿到对应模块的 ResourceManager
            var resourceManager = GetResourceManager(modulePath);

            // 如果模块找不到，直接返回 fullKey（便于暴露漏配）
            if (resourceManager is null)
                return fullKey;

            var culture = GetCultureFromHeader();

            // 1) 按请求语言读取
            var value = resourceManager.GetString(resxKey, culture);

            // 2) zh-CN 兜底
            if (string.IsNullOrEmpty(value))
                value = resourceManager.GetString(resxKey, new CultureInfo("zh-CN"));

            // 3) en-US 兜底
            if (string.IsNullOrEmpty(value))
                value = resourceManager.GetString(resxKey, new CultureInfo("en-US"));

            // 4) 全部没有 → 返回 fullKey（更好定位是哪个模块哪个key漏了）
            if (string.IsNullOrEmpty(value))
                return fullKey;

            // 5) 安全格式化
            try
            {
                return args is { Length: > 0 }
                    ? string.Format(value, args)
                    : value;
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// 获取多语言文案（显式指定语言）
        /// </summary>
        /// <param name="fullKey"></param>
        /// <param name="language"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string ReturnMsg(string fullKey, string language, params object?[] args)
        {
            if (string.IsNullOrWhiteSpace(fullKey))
                return string.Empty;

            // 解析：模块路径 + resxKey
            var (modulePath, resxKey) = ResolveModuleAndResxKey(fullKey);

            // 拿到对应模块的 ResourceManager
            var resourceManager = GetResourceManager(modulePath);

            // 如果模块找不到，直接返回 fullKey（便于暴露漏配）
            if (resourceManager is null)
                return fullKey;

            // 优先使用显式指定的语言，否则从 Header 解析
            CultureInfo culture;
            if (!string.IsNullOrWhiteSpace(language))
            {
                try
                {
                    culture = new CultureInfo(language);
                }
                catch (CultureNotFoundException)
                {
                    culture = GetCultureFromHeader();
                }
            }
            else
            {
                culture = GetCultureFromHeader();
            }

            // 1) 按指定/请求语言读取
            var value = resourceManager.GetString(resxKey, culture);

            // 2) zh-CN 兜底
            if (string.IsNullOrEmpty(value))
                value = resourceManager.GetString(resxKey, new CultureInfo("zh-CN"));

            // 3) en-US 兜底
            if (string.IsNullOrEmpty(value))
                value = resourceManager.GetString(resxKey, new CultureInfo("en-US"));

            // 4) 全部没有 → 返回 fullKey（更好定位是哪个模块哪个key漏了）
            if (string.IsNullOrEmpty(value))
                return fullKey;

            // 5) 安全格式化
            try
            {
                return args is { Length: > 0 }
                    ? string.Format(value, args)
                    : value;
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// 将 fullKey 拆分为模块路径和资源键
        /// </summary>
        private static (string modulePath, string resxKey) ResolveModuleAndResxKey(string fullKey)
        {
            var lastDotIndex = fullKey.LastIndexOf('.');
            if (lastDotIndex <= 0 || lastDotIndex == fullKey.Length - 1)
            {
                // 没有模块路径，按全部当 resxKey（不建议，但做容错）
                return (string.Empty, fullKey);
            }

            var modulePath = fullKey[..lastDotIndex];
            var resxKey = fullKey[(lastDotIndex + 1)..];
            return (modulePath, resxKey);
        }

        /// <summary>
        /// 根据模块路径获取对应的 ResourceManager（带缓存）
        /// </summary>
        private static ResourceManager? GetResourceManager(string modulePath)
        {
            if (string.IsNullOrWhiteSpace(modulePath))
                return null;

            var baseName = $"SystemAdmin.Localization.{modulePath}.Messages";

            // 使用缓存：相同模块只创建一次 ResourceManager
            return _rmCache.GetOrAdd(baseName, bn =>
            {
                // ResourceManager 即使 baseName 不存在也可能被创建，
                // 真正是否存在要等 GetString 时才知道（会返回 null）
                return new ResourceManager(bn, _localizationAssembly);
            });
        }

        /// <summary>
        /// 从 Accept-Language 请求头解析 CultureInfo
        /// </summary>
        private CultureInfo GetCultureFromHeader()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var langHeader = httpContext?.Request.Headers["Accept-Language"].ToString();

            if (string.IsNullOrWhiteSpace(langHeader))
                return new CultureInfo("zh-CN");

            // 示例：zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7
            var first = langHeader.Split(',')[0].Trim();

            try
            {
                return new CultureInfo(first);
            }
            catch
            {
                return new CultureInfo("zh-CN");
            }
        }
    }
}
