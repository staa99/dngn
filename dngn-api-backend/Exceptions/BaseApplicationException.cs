using System;

namespace DngnApiBackend.Exceptions
{
    public abstract class BaseApplicationException : ApplicationException
    {
        protected BaseApplicationException(string code, string message, int statusCode) : base(message)
        {
            Code       = code;
            StatusCode = statusCode;
        }

        public string Code { get; }
        public int StatusCode { get; }
    }
}