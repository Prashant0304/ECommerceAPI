using ECommerceAPI.Services;

namespace ECommerceAPI.Middleware
{
    public class DecryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public DecryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, EncryptionService encryptionService)
        {
            var routeValues = context.Request.RouteValues;

            foreach (var key in routeValues.Keys.ToList())
            {
                if (key.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    var value = routeValues[key]?.ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        
                        if (int.TryParse(value, out _))
                            continue;

                        try
                        {
                            var decrypted = encryptionService.Decrypt(value);
                            routeValues[key] = int.Parse(decrypted);
                        }
                        catch
                        {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("Invalid encrypted id");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}