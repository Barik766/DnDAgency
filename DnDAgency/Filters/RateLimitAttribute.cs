using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace DnDAgency.Api.Filters
{
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitAttribute(int maxRequests = 5, int timeWindowMinutes = 1)
        {
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"rate_limit_{ipAddress}";

            if (cache.TryGetValue(key, out List<DateTime>? requests))
            {
                var validRequests = requests!.Where(r => DateTime.UtcNow - r < _timeWindow).ToList();

                if (validRequests.Count >= _maxRequests)
                {
                    context.Result = new TooManyRequestsResult("Too many requests. Please try again later.");
                    return;
                }

                validRequests.Add(DateTime.UtcNow);
                cache.Set(key, validRequests, _timeWindow);
            }
            else
            {
                cache.Set(key, new List<DateTime> { DateTime.UtcNow }, _timeWindow);
            }

            base.OnActionExecuting(context);
        }
    }

    public class TooManyRequestsResult : IActionResult
    {
        private readonly string _message;

        public TooManyRequestsResult(string message)
        {
            _message = message;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync(_message);
        }
    }
}