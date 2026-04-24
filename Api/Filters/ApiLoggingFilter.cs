using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaProdutos.Filters
{
    public class ApiLoggingFilter : IActionFilter

    {
        public readonly ILogger<ApiLoggingFilter> _logger;

        public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // executado após a ação do controlador ser executada
            _logger.LogInformation("##### Exectutando -> OnActionExecuted");
            _logger.LogInformation("############################################");
            _logger.LogInformation($" {DateTime.Now.ToLongTimeString}");
            _logger.LogInformation($"Status Code : {context.HttpContext.Response.StatusCode}");
            _logger.LogInformation("############################################");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // executado antes da ação do controlador ser executada
            _logger.LogInformation("##### Exectutando -> OnActionExecuting");
            _logger.LogInformation("############################################");
            _logger.LogInformation($" {DateTime.Now.ToLongTimeString}");
            _logger.LogInformation($"ModelState : {context.ModelState.IsValid}");
        }
    }
}
