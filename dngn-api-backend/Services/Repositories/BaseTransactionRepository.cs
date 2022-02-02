using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public abstract class BaseTransactionRepository<TTransaction> : BaseRepository<TTransaction>, ITransactionRepository
        where TTransaction : BaseTransactionModel, new()
    {
        protected BaseTransactionRepository(IMongoDatabase database, string collectionName) : base(
            database, collectionName)
        {
        }

        public virtual async Task<ObjectId> CreateTransactionAsync<TDto>(TDto dto) where TDto : CreateTransactionDto
        {
            if (dto.Amount < 0)
            {
                throw new UserException("INVALID_AMOUNT", "The transaction amount is invalid");
            }

            var transaction = MapCreateTransactionDto(dto);
            transaction.DateCreated = DateTimeOffset.UtcNow;
            await Collection.InsertOneAsync(transaction);
            return transaction.Id;
        }

        public async Task<TransactionDto?> GetTransactionAsync(ObjectId id)
        {
            var cursor = await Collection.FindAsync(FilterBuilder.Eq(t => t.Id, id));
            return await GetTransactionFromCursorAsync(cursor);
        }

        public async Task<TransactionDto?> GetTransactionByBankReferenceAsync(string bankReference)
        {
            var cursor = await Collection.FindAsync(FilterBuilder.Eq(t => t.BankTransactionId, bankReference));
            return await GetTransactionFromCursorAsync(cursor);
        }

        public async Task<TransactionDto?> GetTransactionByProviderReferenceAsync(TransactionProvider provider,
            string providerReference)
        {
            var providerFilter = FilterBuilder.Eq(t => t.Provider, provider);
            var referenceFilter = FilterBuilder.Eq(t => t.ProviderTransactionId, providerReference);
            var cursor = await Collection.FindAsync(FilterBuilder.And(providerFilter, referenceFilter));
            return await GetTransactionFromCursorAsync(cursor);
        }

        public async Task<TransactionDto?> GetTransactionByReferenceAsync(Guid transactionReference)
        {
            var cursor =
                await Collection.FindAsync(FilterBuilder.Eq(t => t.InternalTransactionId, transactionReference));
            return await GetTransactionFromCursorAsync(cursor);
        }

        public virtual async Task UpdateTransactionStatusAsync(ObjectId transactionId, TransactionStatus status, long providerFees)
        {
            var updates = new List<UpdateDefinition<TTransaction>>
            {
                UpdateBuilder.Set(a => a.Status, status),
                UpdateBuilder.Set(a => a.ProviderFees, providerFees),
            };
            
            if (status is TransactionStatus.Successful or TransactionStatus.Failed)
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

        public async Task UpdateTransactionBlockchainStatusAsync(ObjectId transactionId, TransactionBlockchainStatus status, string txHash)
        {
            var updates = new List<UpdateDefinition<TTransaction>>
            {
                UpdateBuilder.Set(a => a.BlockchainStatus, status),
                UpdateBuilder.Set(a => a.BlockchainTransactionHash, txHash),
            };
            
            var updateDefinition = BuildUpdate(updates);

            var updateResult = await Collection.UpdateOneAsync(FilterById(transactionId), updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new UserException("TRANSACTION_NOT_FOUND", "Transaction not found");
            }
        }

        protected TTransaction MapCreateDtoCommonFields<TDto>(TDto dto) where TDto : CreateTransactionDto
        {
            var transaction = new TTransaction
            {
                InternalTransactionId     = dto.InternalTransactionId,
                BankTransactionId         = dto.BankTransactionId,
                ProviderTransactionId     = dto.ProviderTransactionId,
                BlockchainTransactionHash = dto.BlockchainTransactionHash,
                BankAccountId             = dto.BankAccountId,
                UserAccountId             = dto.UserAccountId,
                Amount                    = dto.Amount,
                ProviderFees              = dto.ProviderFees,
                TotalPlatformFees         = dto.TotalPlatformFees,
                TransactionType           = dto.TransactionType,
                Provider                  = dto.Provider,
                Status                    = dto.Status,
                BlockchainStatus          = dto.BlockchainStatus,
                DateCreated               = DateTimeOffset.UtcNow
            };

            return transaction;
        }

        protected abstract TTransaction MapCreateTransactionDto<TDto>(TDto dto) where TDto : CreateTransactionDto;

        private static async Task<TransactionDto?> GetTransactionFromCursorAsync(IAsyncCursor<TTransaction>? cursor)
        {
            if (cursor == default)
            {
                return default;
            }

            var model = await cursor.FirstOrDefaultAsync();
            if (model == default)
            {
                return default;
            }

            return new TransactionDto
            {
                Id                        = model.Id,
                BankAccountId             = model.BankAccountId,
                Amount                    = model.Amount,
                ProviderFees              = model.ProviderFees,
                TotalPlatformFees         = model.TotalPlatformFees,
                Status                    = model.Status,
                BlockchainStatus          = model.BlockchainStatus,
                TransactionType           = model.TransactionType,
                BankTransactionId         = model.BankTransactionId,
                UserAccountId             = model.UserAccountId,
                InternalTransactionId     = model.InternalTransactionId,
                ProviderTransactionId     = model.ProviderTransactionId,
                BlockchainTransactionHash = model.BlockchainTransactionHash,
                DateCreated               = model.DateCreated,
                DateCompleted             = model.DateCompleted
            };
        }
    }
}