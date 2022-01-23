using System;
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


        protected TTransaction MapCreateDtoCommonFields<TDto>(TDto dto) where TDto : CreateTransactionDto
        {
            if (dto.ProviderTransactionId == null)
            {
                throw new UserException("INVALID_TRANSACTION", "No tracking transaction ID on provider");
            }

            var transaction = new TTransaction
            {
                InternalTransactionId = dto.InternalTransactionId,
                BankTransactionId     = dto.BankTransactionId,
                ProviderTransactionId = dto.ProviderTransactionId,
                BankAccountId         = dto.BankAccountId,
                UserAccountId         = dto.UserAccountId,
                Amount                = dto.Amount,
                ProviderFees          = dto.ProviderFees,
                TotalPlatformFees     = dto.TotalPlatformFees,
                TransactionType       = dto.TransactionType,
                Provider              = dto.Provider,
                Status                = dto.Status,
                DateCreated           = DateTimeOffset.UtcNow
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
                Id                    = model.Id,
                BankAccountId         = model.BankAccountId,
                Amount                = model.Amount,
                ProviderFees          = model.ProviderFees,
                TotalPlatformFees     = model.TotalPlatformFees,
                Status                = model.Status,
                TransactionType       = model.TransactionType,
                BankTransactionId     = model.BankTransactionId,
                UserAccountId         = model.UserAccountId,
                InternalTransactionId = model.InternalTransactionId,
                ProviderTransactionId = model.ProviderTransactionId,
                DateCreated           = model.DateCreated,
                DateCompleted         = model.DateCompleted
            };
        }
    }
}