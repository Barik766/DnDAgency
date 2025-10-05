using System.Text;
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
            // Пропускаем Swagger UI
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Только оборачиваем JSON успешные ответы
                if (context.Response.StatusCode >= 200 &&
                    context.Response.StatusCode < 300 &&
                    context.Response.ContentType?.Contains("application/json") == true &&
                    responseBody.Length > 0)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                    object? data = null;
                    try
                    {
                        data = JsonSerializer.Deserialize<object>(responseBodyText);
                    }
                    catch
                    {
                        data = responseBodyText;
                    }

                    var wrappedResponse = new
                    {
                        Success = true,
                        Data = data,
                        Message = "Request completed successfully"
                    };

                    var jsonResponse = JsonSerializer.Serialize(wrappedResponse);
                    var bytes = Encoding.UTF8.GetBytes(jsonResponse);

                    context.Response.Body = originalBodyStream;
                    context.Response.ContentLength = bytes.Length;
                    await context.Response.Body.WriteAsync(bytes);
                }
                else
                {
                    // Копируем оригинальный ответ
                    responseBody.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = originalBodyStream;
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}