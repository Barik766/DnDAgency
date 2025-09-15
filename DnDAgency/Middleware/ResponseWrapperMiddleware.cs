using System.Text.Json;

namespace DnDAgency.Api.Middleware
{
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Wrap successful responses
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 &&
                context.Response.ContentType?.Contains("application/json") == true)
            {
                var wrappedResponse = new
                {
                    Success = true,
                    Data = responseBodyText.StartsWith("{") || responseBodyText.StartsWith("[")
                        ? JsonSerializer.Deserialize<object>(responseBodyText)
                        : responseBodyText,
                    Message = "Request completed successfully"
                };

                var jsonResponse = JsonSerializer.Serialize(wrappedResponse);
                context.Response.ContentLength = null;
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsync(responseBodyText);
            }
        }
    }
}