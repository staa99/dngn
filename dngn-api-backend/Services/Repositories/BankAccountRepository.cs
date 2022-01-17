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
    public class BankAccountRepository : BaseRepository<BankAccount>, IBankAccountRepository
    {
        private readonly IBankRepository _bankRepository;

        public BankAccountRepository(IMongoDatabase database, IBankRepository bankRepository) : base(database,
            DngnMongoSchema.BankAccountCollection)
        {
            _bankRepository = bankRepository;
        }

        public async Task<ObjectId> CreateBankAccountAsync(CreateBankAccountDto dto)
        {
            if (dto.AccountNumber == null)
            {
                throw new UserException("ACCOUNT_NUMBER_REQUIRED", "Account number is required");
            }

            if (dto.AccountName == null)
            {
                throw new UserException("ACCOUNT_NAME_REQUIRED", "Account name is required");
            }

            var bank = new BankAccount
            {
                AccountName   = dto.AccountName,
                AccountNumber = dto.AccountNumber,
                BankId        = dto.BankId,
                UserId        = dto.UserId,
                IsVirtual     = dto.IsVirtual,
                Metadata      = dto.Metadata,
                DateCreated   = DateTimeOffset.UtcNow
            };
            await Collection.InsertOneAsync(bank);
            return bank.Id;
        }

        public async Task<BankAccountDto?> GetBankAccountAsync(ObjectId id)
        {
            var cursor = await Collection.FindAsync(FilterById(id));
            return await LoadBankAccountAsync(cursor);
        }

        public async Task<BankAccountDto?> GetBankAccountAsync(TransactionProvider provider, string providerReference)
        {
            var providerField =
                new StringFieldDefinition<BankAccount, TransactionProvider>(
                    $"{nameof(BankAccount.Metadata)}.{nameof(BankAccountMetaKey.Provider)}");
            var providerReferenceField =
                new StringFieldDefinition<BankAccount, string>(
                    $"{nameof(BankAccount.Metadata)}.{nameof(BankAccountMetaKey.ProviderAccountReference)}");

            var cursor = await Collection.FindAsync(FilterBuilder.And(FilterBuilder.Eq(providerField, provider),
                FilterBuilder.Eq(providerReferenceField, providerReference)));

            return await LoadBankAccountAsync(cursor);
        }

        private async Task<BankAccountDto?> LoadBankAccountAsync(IAsyncCursor<BankAccount> cursor)
        {
            var accountTask = cursor.FirstOrDefaultAsync();
            var bankAccount = accountTask != null ? await accountTask : null;

            if (bankAccount == null)
            {
                return null;
            }

            var bank = bankAccount.BankId.HasValue
                ? await _bankRepository.GetBankAsync(bankAccount.BankId.Value)
                : null;

            var bankName = bank != null
                ? bank.Name
                : bankAccount.Metadata.ContainsKey(BankAccountMetaKey.BankName)
                    ? bankAccount.Metadata[BankAccountMetaKey.BankName]
                    : null;
            return new BankAccountDto
            {
                Id            = bankAccount.Id,
                UserId        = bankAccount.UserId,
                AccountName   = bankAccount.AccountName,
                AccountNumber = bankAccount.AccountNumber,
                IsVirtual     = bankAccount.IsVirtual,
                Bank          = bank,
                BankName      = bankName,
                Metadata      = bankAccount.Metadata
            };
        }
    }
}