using System.Net;

namespace DngnApiBackend.Exceptions
{
    public class UserException : BaseApplicationException
    {
        public UserException(string code, string message) : base(code, message, (int) HttpStatusCode.BadRequest)
        {
        }
    }
}