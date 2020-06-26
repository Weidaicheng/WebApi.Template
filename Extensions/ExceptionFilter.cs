using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Template.Models;

namespace WebApi.Template.Extensions
{
    public class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is Exception exception)
            {
                context.Result = new ObjectResult(new Result<string>(HttpStatusCode.InternalServerError, exception.Message))
                {
                    StatusCode = 200,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}