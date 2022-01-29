using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.InitiateTransfer;
using DngnApiBackend.Services.Dto;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class WithdrawalRepository : BaseTransactionRepository<Withdrawal>, IWithdrawalRepository
    {
        private readonly ILogger<WithdrawalRepository> _logger;

        public WithdrawalRepository(IMongoDatabase database, ILogger<WithdrawalRepository> logger) : base(database,
            DngnMongoSchema.WithdrawalCollection)
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
        
        public async Task UpdateTransactionForTransferAsync(ObjectId transactionId, BaseTransferOutput transferOutput)
        {
            var updates = new List<UpdateDefinition<Withdrawal>>
            {
                UpdateBuilder.Set(a => a.Status, transferOutput.Status),
                UpdateBuilder.Set(a => a.ProviderFees, transferOutput.TransferFees),
                UpdateBuilder.Set(a => a.ProviderTransactionId, transferOutput.ProviderTransactionReference),
                UpdateBuilder.Set(a => a.BankTransactionId, transferOutput.ProviderBankReference),
            };
            
            if (transferOutput.Status is TransactionStatus.Successful or TransactionStatus.Failed)
            {
                updates.Add(UpdateBuilder.Set(a => a.DateCompleted, DateTimeOffset.UtcNow));
            }
            var updateDefinition = BuildUpdate(updates);

            var updateResult = await Collection.UpdateOneAsync(FilterById(transactionId), updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new UserException("TRANSACTION_NOT_FOUND", "Transaction not found");
            }
        }
    }
}