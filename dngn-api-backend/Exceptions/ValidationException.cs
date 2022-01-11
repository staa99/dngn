using System;

namespace DngnApiBackend.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}