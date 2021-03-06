using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Template.Models;
using Microsoft.Extensions.Logging;

namespace WebApi.Template.Extensions
{
    public class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is Exception exception)
            {
                _logger.LogError(exception, exception.Message);

                context.Result = new ObjectResult(new Result<string>(HttpStatusCode.InternalServerError, exception.Message))
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}