using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 当前登录用户信息访问器
    /// </summary>
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 用户Id 的 Claim 类型
        /// </summary>
        public const string ClaimUserId = "uid";

        /// <summary>
        /// 用户工号的 Claim 类型
        /// </summary>
        public const string ClaimUserNo = "uno";

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// 登录用户Id
        /// </summary>
        public long UserId => GetLongClaim(ClaimUserId);

        /// <summary>
        /// 登录用户工号
        /// </summary>
        public string UserNo => GetStringClaim(ClaimUserNo);

        /// <summary>
        /// 当前请求的 ClaimsPrincipal
        /// </summary>
        public ClaimsPrincipal? ClaimsPrincipal => Principal;

        private string GetStringClaim(string claimType)
        {
            var value = Principal?.FindFirst(claimType)?.Value;
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        private long GetLongClaim(string claimType)
        {
            var value = Principal?.FindFirst(claimType)?.Value;
            return long.TryParse(value, out var id) ? id : 0L;
        }
    }
}
