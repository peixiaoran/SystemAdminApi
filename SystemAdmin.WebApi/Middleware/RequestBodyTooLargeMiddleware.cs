using Microsoft.Extensions.Options;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.WebApi.Middleware
{
    /// <summary>
    /// 请求体超过 Kestrel/表单读取上限时，框架会抛出类似
    /// "Failed to read the request form. Request body too large. The max request body size is 30000000 bytes."
    /// 的原始异常，字节数对用户不友好，这里统一转换为按 MB 展示的多语言提示
    /// </summary>
    public class RequestBodyTooLargeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyTooLargeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<FileUploadOptions> fileUpload, LocalizationService localization)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.Response.HasStarted && IsBodyTooLarge(ex))
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var message = localization.ReturnMsg("FormBusiness.Forms.RequestBodyTooLarge", fileUpload.Value.MaxSizeMB);
                await context.Response.WriteAsJsonAsync(Result<object>.Failure(400, message));
            }
        }

        private static bool IsBodyTooLarge(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e.Message.Contains("body too large", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
