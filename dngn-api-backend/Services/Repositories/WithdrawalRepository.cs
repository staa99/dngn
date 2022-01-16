using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class WithdrawalRepository: BaseTransactionRepository<Withdrawal>, IWithdrawalRepository
    {
        private readonly ILogger<WithdrawalRepository> _logger;

        public WithdrawalRepository(IMongoDatabase database, ILogger<WithdrawalRepository> logger) : base(database, DngnMongoSchema.WithdrawalCollection)
        {
            _logger = logger;
        }

        protected override Withdrawal MapCreateTransactionDto<TDto>(TDto dto)
        {
            if (dto is CreateWithdrawalDto)
            {
                return MapCreateDtoCommonFields(dto);
            }

            _logger.LogError("Create transaction has been called with the wrong type: {Type}", typeof(TDto));
            throw new ServiceException("INVALID_DATA_TYPE", "The data is not valid for withdrawal transactions");
        }
    }
}