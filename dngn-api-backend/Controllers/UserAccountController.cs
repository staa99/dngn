using System.Threading.Tasks;
using DngnApiBackend.ApiModels;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;
using DngnApiBackend.Integrations.VirtualAccounts;
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

        [HttpPost("deposit-account")]
        public async Task<IActionResult> GenerateDepositAccountAsync([FromBody] GenerateDepositAccountModel model, [FromServices] IVirtualAccountCreator virtualAccountCreator)
        {
            AssertValidModelState();
            _logger.LogTrace("Generating deposit account for User:{Id}", CurrentUserId);

            var currentUser = await _userAccountRepository.GetAccountAsync(CurrentUserId);
            if (currentUser == null)
            {
                _logger.LogError("The current user does not exist");
                return Unauthorized();
            }

            if (currentUser.DepositAccount != null)
            {
                _logger.LogInformation("User:{Id} already has a deposit account", CurrentUserId);
                ThrowUserError("DEPOSIT_ACCOUNT_EXISTS", "Cannot generate an additional deposit account");
                return StatusCode(500);
            }

            var result = await virtualAccountCreator.CreateVirtualAccountAsync(CurrentUserId,
                new CreateVirtualAccountInput
                {
                    Email     = model.EmailAddress,
                    BVN       = model.BVN,
                    FirstName = currentUser.FirstName,
                    LastName  = currentUser.LastName,
                    Provider  = Constants.VirtualAccountProvider
                });

            return result.Status
                ? Ok(new
                {
                    status  = "success",
                    message = result.Message,
                    data = new
                    {
                        bank          = result.BankName,
                        accountName   = result.AccountName,
                        accountNumber = result.AccountNumber
                    }
                })
                : StatusCode(500, new
                {
                    status = "failed",
                    error = new
                    {
                        code    = "ACCOUNT_GENERATION_FAILED",
                        message = result.Message
                    }
                });
        }
    }
}