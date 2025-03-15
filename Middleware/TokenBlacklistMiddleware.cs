using UltraStrore.Repository;

namespace UltraStrore.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlacklistService _blacklistService;

        public TokenBlacklistMiddleware(RequestDelegate next, ITokenBlacklistService blacklistService)
        {
            _next = next;
            _blacklistService = blacklistService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                if (await _blacklistService.IsTokenBlacklisted(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token đã bị vô hiệu hóa.");
                    return;
                }
            }
            await _next(context);
        }
    }

    // Extension để thêm middleware
    public static class TokenBlacklistMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenBlacklistMiddleware>();
        }
    }
}
