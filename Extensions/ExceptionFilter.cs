using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Template.Models;
using Microsoft.Extensions.Logging;
using WebApi.Template.Models.Enums;
using System.Collections.Generic;

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

        public void OnActionExecuting(ActionExecutingContext context) 
        { 
            if(!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, IEnumerable<string>>();
                foreach(var ms in context.ModelState)
                {
                    var messages = new List<string>();
                    foreach(var error in ms.Value.Errors)
                    {
                        messages.Add(error.ErrorMessage);
                    }
                    errors.Add(ms.Key, messages);
                }
                context.Result = new ObjectResult(new Result<Dictionary<string, IEnumerable<string>>>(StatusCode.ModelValidationFailed, "Model validation failed", errors))
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is WebApiException webapiException)
            {
                _logger.LogError(webapiException, webapiException.Message);

                context.Result = new ObjectResult(new Result<string>(webapiException.Code ?? StatusCode.Error, webapiException.Message))
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
                context.ExceptionHandled = true;
            }
            else if (context.Exception is Exception exception)
            {
                _logger.LogError(exception, exception.Message);

                context.Result = new ObjectResult(new Result<string>(StatusCode.Error, exception.Message))
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}