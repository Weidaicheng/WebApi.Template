using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApi.Template.Models;

using StatusCodeEnum = WebApi.Template.Models.Enums.StatusCode;

namespace WebApi.Template.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/Error")]
        public Result<string> Error(string message)
        {
            _logger.LogError(message);

            return new Result<string>(StatusCodeEnum.Error, message);
        }
    }
}