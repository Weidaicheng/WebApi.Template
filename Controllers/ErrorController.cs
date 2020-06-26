using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Template.Models;

namespace WebApi.Template.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [ApiVersion("1.0")]
        public Result<string> Error(string message)
        {
            return new Result<string>(HttpStatusCode.InternalServerError, message);
        }
    }
}