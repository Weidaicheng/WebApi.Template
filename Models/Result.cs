using System.Net;

namespace WebApi.Template.Models
{
    public class Result<T>
    {
        public Result(HttpStatusCode code, string message, T data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public Result(HttpStatusCode code, string message) : this(code, message, default(T))
        { }

        public Result(T data) : this(HttpStatusCode.OK, string.Empty, data)
        { }

        public Result() : this(default(T))
        { }

        public HttpStatusCode Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }
}