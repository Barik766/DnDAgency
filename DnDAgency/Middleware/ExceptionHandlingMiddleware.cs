using System.Net;
using System.Text.Json;

namespace DnDAgency.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/json";


            var response = exception switch
            {
                ArgumentException => new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message
                },
                KeyNotFoundException => new
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Resource not found"
                },
                UnauthorizedAccessException => new
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized"
                },
                InvalidOperationException => new
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = exception.Message 
                },
                _ => new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing your request"
                }
            };

            context.Response.StatusCode = response.StatusCode;
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

    }
}