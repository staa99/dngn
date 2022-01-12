using System.Net;

namespace DngnApiBackend.Exceptions
{
    public class PermissionDeniedException : BaseApplicationException
    {
        public PermissionDeniedException(string code, string message) : base(code, message,
            (int) HttpStatusCode.Forbidden)
        {
        }
    }
}