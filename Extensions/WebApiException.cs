using System;
using WebApi.Template.Models.Enums;

namespace WebApi.Template.Extensions
{
    public class WebApiException : Exception
    {
        public StatusCode? Code { get; set; }
    }
}