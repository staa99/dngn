using System;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly IBankRepository _bankRepository;
        private readonly IMongoCollection<BankAccount> _collection;

        public BankAccountRepository(IMongoDatabase database, IBankRepository bankRepository)
        {
            _bankRepository = bankRepository;
            _collection     = database.GetCollection<BankAccount>(DngnMongoSchema.BankAccountCollection);
        }

        public async Task<ObjectId> CreateBankAccountAsync(CreateBankAccountDto dto)
        {
            if (dto.AccountNumber == null)
            {
                throw new ValidationException("Account number is required");
            }

            if (dto.AccountName == null)
            {
                throw new ValidationException("Account name is required");
            }

            var bank = new BankAccount
            {
                AccountName   = dto.AccountName,
                AccountNumber = dto.AccountNumber,
                BankId        = dto.BankId,
                IsVirtual     = dto.IsVirtual,
                DateCreated   = DateTimeOffset.UtcNow
            };
            await _collection.InsertOneAsync(bank);
            return bank.Id;
        }

        public async Task<BankAccountDto?> GetBankAccountAsync(ObjectId id)
        {
            var cursor = await _collection.FindAsync(a => a.Id == id);
            var accountTask = cursor?.FirstOrDefaultAsync();
            var bankAccount = accountTask != null ? await accountTask : null;

            if (bankAccount == null)
            {
                return null;
            }

            var bank = await _bankRepository.GetBankAsync(bankAccount.BankId);
            return new BankAccountDto
            {
                Id            = bankAccount.Id,
                AccountName   = bankAccount.AccountName,
                AccountNumber = bankAccount.AccountNumber,
                IsVirtual     = bankAccount.IsVirtual,
                Bank          = bank
            };
        }
    }
}