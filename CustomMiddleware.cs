using TabFusionRMS.ContextHelp;

namespace TabFusionRMS.WebCS
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // IMessageWriter is injected into InvokeAsync
        public async Task InvokeAsync(HttpContext httpContext, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IHttpContextAccessor _httpContextAccessor)
        {
            TabFusionRMS.ContextHelp.ContextService.httpContext = httpContext;
            TabFusionRMS.ContextHelp.ContextService.basedirectory = webHostEnvironment.ContentRootPath;
            TabFusionRMS.ContextHelp.ContextService.configuration = configuration;
            TabFusionRMS.ContextHelp.ContextService.httpContextAccessor = _httpContextAccessor;
            await _next(httpContext);
        }
    }

    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomMiddleware>();
        }
    }
}
