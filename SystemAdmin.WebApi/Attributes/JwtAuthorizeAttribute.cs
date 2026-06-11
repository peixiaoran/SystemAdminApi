using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 标记了 [AllowAnonymous] 的 Action/Controller 跳过验证
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
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
}
