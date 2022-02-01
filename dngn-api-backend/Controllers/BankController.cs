using System.Threading.Tasks;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.BankUtilities;
using DngnApiBackend.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DngnApiBackend.Controllers
{
    [AllowAnonymous]
    [Route("banks")]
    public class BankController : BaseController
    {
        private readonly IBankRepository _bankRepository;
        private readonly IBankAccountNameResolver _bankAccountNameResolver;

        public BankController(IBankRepository bankRepository, IBankAccountNameResolver bankAccountNameResolver)
        {
            _bankRepository               = bankRepository;
            _bankAccountNameResolver = bankAccountNameResolver;
        }


        [HttpGet("")]
        public async Task<IActionResult> GetBanksAsync(string? query)
        {
            var banks = await _bankRepository.GetBanksAsync(query);
            return Ok(new
            {
                status  = "success",
                message = "Banks loaded successfully",
                data    = banks
            });
        }


        [HttpGet("account-name")]
        public async Task<IActionResult> ResolveAccountNameAsync(string bankId, string accountNumber)
        {
            AssertValidModelState();
            if (!ObjectId.TryParse(bankId, out var bankObjectId))
            {
                throw new UserException("BANK_ID_INVALID", "The ID is not in the correct format");
            }
            
            var bank = await _bankRepository.GetBankAsync(bankObjectId);
            if (bank == null)
            {
                throw new UserException("BANK_NOT_FOUND", "The ID does not match any known bank");
            }

            var result = await _bankAccountNameResolver.ResolveBankAccountNameAsync(accountNumber,
                bank.Metadata[_bankAccountNameResolver.BankCodeMetaKey]);
            
            return Ok(new
            {
                status  = "success",
                message = "Account resolved successfully",
                data    = result
            });
        }
    }
}