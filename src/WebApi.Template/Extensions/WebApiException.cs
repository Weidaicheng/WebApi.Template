using System;
using WebApi.Template.Models.Enums;

namespace WebApi.Template.Extensions
{
    public class WebApiException : Exception
    {
        public StatusCode? Code { get; set; }

        public WebApiException(StatusCode code, string message)
            : base(message)
        { 
            Code = code;
        }

        public WebApiException(string message)
            : this(StatusCode.Error, message)
        { }
    }
}