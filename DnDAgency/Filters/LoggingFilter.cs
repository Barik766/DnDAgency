using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace DnDAgency.Api.Filters
{
    public class LoggingFilter : IActionFilter
    {
        private readonly ILogger<LoggingFilter> _logger;

        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName;
            var arguments = JsonSerializer.Serialize(context.ActionArguments);

            _logger.LogInformation("Executing {ControllerName}.{ActionName} with arguments: {Arguments}",
                controllerName, actionName, arguments);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName;
            var statusCode = context.HttpContext.Response.StatusCode;

            _logger.LogInformation("Executed {ControllerName}.{ActionName} with status code: {StatusCode}",
                controllerName, actionName, statusCode);
        }
    }
}