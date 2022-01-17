using System.Threading.Tasks;
using DngnApiBackend.ApiModels;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using DngnApiBackend.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace DngnApiBackend.Controllers
{
    [Route("users")]
    public class UserAccountController : BaseController
    {
        private readonly ILogger<UserAccountController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;

        public UserAccountController(ILogger<UserAccountController> logger,
            IUserAccountRepository userAccountRepository)
        {
            _logger                = logger;
            _userAccountRepository = userAccountRepository;
        }


        [HttpGet("profile")]
        public async Task<IActionResult> GetUserAccountAsync()
        {
            var account = await _userAccountRepository.GetAccountAsync(CurrentUserId);
            if (account != null)
            {
                return Ok(new
                {
                    status  = "success",
                    message = "Profile loaded successfully",
                    data    = account
                });
            }

            _logger.LogError("Failed to load account for current user");
            throw new ServiceException("UNEXPECTED_ERROR", "Failed to load profile");
        }


        [HttpPost("withdrawal-account")]
        public async Task<IActionResult> SetWithdrawalAccountAsync([FromBody] CreateWithdrawalAccountModel model)
        {
            AssertValidModelState();
            if (!ObjectId.TryParse(model.BankId, out var bankId))
            {
                throw new UserException("INVALID_BANK_ID", "The specified bank is not valid");
            }

            _logger.LogTrace("Setting withdrawal account for User:{Id}", CurrentUserId);
            await _userAccountRepository.SetWithdrawalBankAccountAsync(CurrentUserId, new CreateBankAccountDto
            {
                AccountName   = model.AccountName,
                AccountNumber = model.AccountNumber,
                BankId        = bankId,
                UserId        = CurrentUserId
            });
            _logger.LogTrace("Set withdrawal account for User:{Id}", CurrentUserId);
            return Ok(new
            {
                status  = "success",
                message = "Withdrawal account updated"
            });
        }
    }
}