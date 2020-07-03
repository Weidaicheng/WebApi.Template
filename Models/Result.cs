using System.Net;
using WebApi.Template.Models.Enums;

namespace WebApi.Template.Models
{
    public class Result<T>
    {
        public Result(StatusCode code, string message, T data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public Result(StatusCode code, string message) : this(code, message, default(T))
        { }

        public Result(T data) : this(StatusCode.Success, string.Empty, data)
        { }

        public Result() : this(default(T))
        { }

        public StatusCode Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }
}