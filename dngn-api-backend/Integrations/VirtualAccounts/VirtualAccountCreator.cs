using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;
using DngnApiBackend.Services.Dto;
using DngnApiBackend.Services.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace DngnApiBackend.Integrations.VirtualAccounts
{
    public class VirtualAccountCreator : IVirtualAccountCreator
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IProviderVirtualAccountCreator _providerVirtualAccountCreator;
        private readonly ILogger<VirtualAccountCreator> _logger;


        public VirtualAccountCreator(IUserAccountRepository userAccountRepository,
            IProviderVirtualAccountCreator providerVirtualAccountCreator,
            ILogger<VirtualAccountCreator> logger)
        {
            _userAccountRepository         = userAccountRepository;
            _providerVirtualAccountCreator = providerVirtualAccountCreator;
            _logger                   = logger;
        }


        public async Task<CreateVirtualAccountOutput> CreateVirtualAccountAsync(string ownerId, CreateVirtualAccountInput input)
        {
            if (!ObjectId.TryParse(ownerId, out var ownerObjectId))
            {
                _logger.LogError("Invalid user ID: An error has occurred in the upstream jwt service");
                throw new ServiceException("INVALID_USER", "The user id provided is invalid");
            }
            
            _logger.LogInformation("Creating a new virtual account");
            var result = await _providerVirtualAccountCreator.CreateVirtualAccountAsync(input);
            if (!result.Status || string.IsNullOrEmpty(result.VirtualAccountId) || string.IsNullOrEmpty(result.AccountNumber) || string.IsNullOrEmpty(result.AccountName))
            {
                _logger.LogInformation("Failed to create virtual account: {Message}", result.Message);
                return result;
            }

            await _userAccountRepository.SetDepositBankAccountAsync(ownerObjectId, new CreateBankAccountDto
            {
                IsVirtual     = true,
                AccountName   = result.AccountName,
                AccountNumber = result.AccountNumber,
                BankId        = null,
                Metadata =
                {
                    [BankAccountMetaKey.Provider]                 = Constants.VirtualAccountProvider,
                    [BankAccountMetaKey.BankName]                 = result.BankName,
                    [BankAccountMetaKey.ProviderAccountReference] = result.VirtualAccountId
                }
            });

            return result;
        }
    }
}