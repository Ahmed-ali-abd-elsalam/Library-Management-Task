using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.MiddleWares
{
    public class GlobalTokenValidationFilter : IAsyncAuthorizationFilter
    {
        private readonly IDistributedCache _cache;

        public GlobalTokenValidationFilter(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            // Only run this logic if the endpoint requires authorization
            var hasAuthorize = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;
            if (!hasAuthorize)
                return;

            var httpContext = context.HttpContext;
            var user = httpContext.User;
            if (httpContext.Request.Path.StartsWithSegments("/api/Auth/refresh"))
                return;

            if (user?.Identity?.IsAuthenticated != true)
                return; // Not authenticated, let [Authorize] handle it

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new JsonResult(new { error = "Missing or invalid Authorization header" })
                {
                    StatusCode = 401
                };
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return;


            var source = httpContext.Request.Headers["User-Agent"].ToString();
            var key = $"{userEmail}-{source}";
            var cachedToken = await _cache.GetStringAsync(key);

            if (cachedToken is null || !cachedToken.Equals(token))
            {
                context.Result = new JsonResult(new { error = "Invalid Token" })
                {
                    StatusCode = 401
                };
            }
        }
    }
}