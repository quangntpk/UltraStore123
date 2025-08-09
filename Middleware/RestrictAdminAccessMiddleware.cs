namespace UltraStrore.Middleware
{
    public class RestrictAdminAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public RestrictAdminAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Admin"))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Not Found");
                    return;
                }
            }

            await _next(context);
        }
    }
}
