using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace dj_api.Authentication
{
    public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiKeyValidation _apiKeyValidation;

        public ApiKeyHandler(IHttpContextAccessor httpContextAccessor, IApiKeyValidation apiKeyValidation)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiKeyValidation = apiKeyValidation;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var userApiKey = _httpContextAccessor?.HttpContext?.Request.Headers["X-API-Key"];
            var requestMethod = httpContext.Request.Method;

            if (string.IsNullOrWhiteSpace(userApiKey))
            {
               // context.Fail();
                DenyAccess(httpContext);
                
                return Task.CompletedTask;
            }
            if (!_apiKeyValidation.IsValidApiKey(userApiKey!, requestMethod))
            {
               // context.Fail();
                DenyAccess(httpContext);
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        private void DenyAccess(HttpContext httpContext)
        {
            if (httpContext != null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.WriteAsync("{\"error\": \"Unauthorized: Invalid API Key\"}").Wait();
                httpContext.Abort();
            }
        }
    }
}
