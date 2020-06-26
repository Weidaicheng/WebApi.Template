using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.Template.Models;

namespace WebApi.Template.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [Route("api/Error")]
        public Result<string> Error(string message)
        {
            return new Result<string>(HttpStatusCode.InternalServerError, message);
        }
    }
}