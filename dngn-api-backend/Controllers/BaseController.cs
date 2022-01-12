using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using DngnApiBackend.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DngnApiBackend.Controllers
{
    public class BaseController : Controller
    {
        protected ObjectId CurrentUserId =>
            ObjectId.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
                ? id
                : throw new UserException("UNAUTHORIZED", "You must be logged in to use this service");
        
        protected virtual void AssertValidModelState()
        {
            if (!ModelState.IsValid)
            {
                throw new UserException("INVALID_REQUEST_DATA",
                    ModelState.FirstOrDefault().Value?.Errors?.FirstOrDefault()?.ErrorMessage ??
                    "The request could not be processed");
            }
        }

        protected virtual void ThrowUserError(string code, string message)
        {
            throw new UserException(code, message);
        }

        protected virtual void ThrowServiceError(string code, string message)
        {
            throw new ServiceException(code, message);
        }
    }
}