using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class DepositRepository: BaseTransactionRepository<Deposit>, IDepositRepository
    {
        private readonly ILogger<DepositRepository> _logger;

        public DepositRepository(IMongoDatabase database, ILogger<DepositRepository> logger) : base(database, DngnMongoSchema.DepositCollection)
        {
            _logger = logger;
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
            deposit.RawWebhookPayload = depositDto.RawWebhookPayload;
            return deposit;
        }
    }
}