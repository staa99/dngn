using System.Net;

namespace DngnApiBackend.Exceptions
{
    public class ServiceException : BaseApplicationException
    {
        public ServiceException(string code, string message) : base(code, message,
            (int) HttpStatusCode.InternalServerError)
        {
        }
    }
}