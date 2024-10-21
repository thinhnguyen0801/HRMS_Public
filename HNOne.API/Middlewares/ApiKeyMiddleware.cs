namespace HNOne.API.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string API_KEY_NAME = "API_KEY";
        public ApiKeyMiddleware(RequestDelegate next) { _next = next; }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(API_KEY_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provider!!!");
                return;
            }
            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetSection($"GlobalConfiguration:{API_KEY_NAME}").Value;
            if (apiKey == null || !apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("UnAuthorized client!!!");
                return;
            }

            // có cách triển khai 1. Thông qua ActionFilter. 2. Thông qua middleware
            // đi vào action method
            await _next(context);
        }
    }
}
