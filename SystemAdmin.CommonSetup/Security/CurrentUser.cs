using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>当前登录用户</summary>
    public class CurrentUser
    {
        /// <summary>用户 Id 的 Claim 类型</summary>
        public const string ClaimUserId = "uid";

        /// <summary>用户工号的 Claim 类型</summary>
        public const string ClaimUserNo = "uno";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>当前请求的 ClaimsPrincipal</summary>
        public ClaimsPrincipal? ClaimsPrincipal => _httpContextAccessor.HttpContext?.User;

        /// <summary>是否已登录</summary>
        public bool IsAuthenticated => ClaimsPrincipal?.Identity?.IsAuthenticated ?? false;

        /// <summary>用户 Id，未登录为 0</summary>
        public long UserId => long.TryParse(GetClaim(ClaimUserId), out var id) ? id : 0L;

        /// <summary>用户工号，未登录为空串</summary>
        public string UserNo => GetClaim(ClaimUserNo) ?? string.Empty;

        private string? GetClaim(string claimType)
        {
            var value = ClaimsPrincipal?.FindFirst(claimType)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
