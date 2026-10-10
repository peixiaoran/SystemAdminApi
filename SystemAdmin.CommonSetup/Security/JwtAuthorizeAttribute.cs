using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string CsrfHeaderName = "X-Requested-With";
    private const string CsrfHeaderValue = "XMLHttpRequest";

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 标记了 [AllowAnonymous] 的 Action/Controller 跳过验证
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return Task.CompletedTask;
        }

        var request = context.HttpContext.Request;

        // 鉴权走 withCredentials + HttpOnly Cookie，简单请求（如 multipart/form-data）不会触发 CORS 预检，
        // Origins 白名单形同虚设；要求带上该自定义头以强制预检，从而落到 CORS Origin 校验，防跨站请求伪造
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method) && !HasCsrfHeader(request))
        {
            context.Result = new JsonResult(Result<bool>.Failure(403, "Forbidden: Missing required request header"))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return Task.CompletedTask;
        }

        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new JsonResult(Result<bool>.Failure(401, "Unauthorized: Invalid or expired token"))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        return Task.CompletedTask;
    }

    private static bool HasCsrfHeader(HttpRequest request)
        => request.Headers.TryGetValue(CsrfHeaderName, out var value) && value == CsrfHeaderValue;
}
