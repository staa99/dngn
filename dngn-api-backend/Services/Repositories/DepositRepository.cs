using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Blockchain;
using DngnApiBackend.Integrations.Blockchain.Outgoing;
using DngnApiBackend.Services.Dto;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class DepositRepository : BaseTransactionRepository<Deposit>, IDepositRepository
    {
        private readonly IBlockchainNotifier _blockchainNotifier;
        private readonly ILogger<DepositRepository> _logger;
        private readonly IUserAccountRepository _userAccountRepository;

        public DepositRepository(IBlockchainNotifier blockchainNotifier, IUserAccountRepository userAccountRepository,
            IMongoDatabase database, ILogger<DepositRepository> logger) : base(database,
            DngnMongoSchema.DepositCollection)
        {
            _blockchainNotifier    = blockchainNotifier;
            _userAccountRepository = userAccountRepository;
            _logger                = logger;
        }

        public override async Task<ObjectId> CreateTransactionAsync<TDto>(TDto dto)
        {
            var result = await base.CreateTransactionAsync(dto);
            if (result == default || dto.Status != TransactionStatus.Successful)
            {
                return result;
            }

            var address = await _userAccountRepository.GetAddressAsync(dto.UserAccountId);
            if (string.IsNullOrEmpty(address))
            {
                _logger.LogCritical("ORPHANED_DEPOSIT: Address for deposit does not exist");
            }
            else
            {
                await _blockchainNotifier.TriggerMinter(
                    new BlockchainOutgoingInstruction(dto.ProviderTransactionId!, address, dto.Amount,
                        dto.TotalPlatformFees));
            }

            return result;
        }

        protected override Deposit MapCreateTransactionDto<TDto>(TDto dto)
        {
            if (dto is not CreateDepositDto depositDto)
            {
                _logger.LogError("Create transaction has been called with the wrong type: {Type}", typeof(TDto));
                throw new ServiceException("INVALID_DATA_TYPE", "The data is not valid for deposit transactions");
            }

            if (string.IsNullOrWhiteSpace(depositDto.RawWebhookPayload))
            {
                throw new ServiceException("WEBHOOK_PAYLOAD_REQUIRED",
                    "The webhook payload is required for deposit transactions");
            }

            var deposit = MapCreateDtoCommonFields(dto);
            deposit.BlockchainStatus  = TransactionBlockchainStatus.Initiated;
            deposit.RawWebhookPayload = depositDto.RawWebhookPayload;
            return deposit;
        }
    }
}