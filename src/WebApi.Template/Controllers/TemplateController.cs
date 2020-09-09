using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Template.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using ActionName = WebApi.Template.Extensions.ActionNameAttribute;
using WebApi.Template.Models;
using System.Net;

namespace WebApi.Template.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class TemplateController : ControllerBase
    {
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(ILogger<TemplateController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ApiVersion("1.0")]
        public Result<string> Hello()
        {
            return new Result<string>("Hello world from version 1.0!");
        }

        [HttpGet]
        [ApiVersion("1.1")]
        [ActionName("Hello")]
        public Result<string> Hello2()
        {
            return new Result<string>("Hello world from version 1.1!");
        }
    }
}
