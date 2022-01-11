using System;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IMongoCollection<UserAccount> _collection;

        public UserAccountRepository(IMongoDatabase database, IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
            _collection            = database.GetCollection<UserAccount>(DngnMongoSchema.UserAccountCollection);
        }

        public async Task<ObjectId> CreateUserAccountAsync(CreateAccountDto dto)
        {
            if (dto.WalletAddress == null)
            {
                throw new ValidationException("Wallet address is required");
            }

            if (dto.FirstName == null)
            {
                throw new ValidationException("First name is required");
            }

            if (dto.LastName == null)
            {
                throw new ValidationException("Last name is required");
            }

            var account = new UserAccount
            {
                FirstName     = dto.FirstName,
                LastName      = dto.LastName,
                WalletAddress = dto.WalletAddress.ToLowerInvariant(),
                Nonce         = Guid.NewGuid(),
                DateCreated   = DateTimeOffset.UtcNow
            };
            await _collection.InsertOneAsync(account);
            return account.Id;
        }

        public async Task SetDepositBankAccountAsync(ObjectId userAccountId, CreateBankAccountDto dto)
        {
            var bankAccountId = await _bankAccountRepository.CreateBankAccountAsync(dto);
            var filterDefinition = Builders<UserAccount>.Filter.Eq(a => a.Id, userAccountId);

            var updateBuilder = new UpdateDefinitionBuilder<UserAccount>();
            var updateDefinition = updateBuilder.Combine(updateBuilder.Set(a => a.DepositBankAccountId, bankAccountId),
                updateBuilder.Set(a => a.DateModified, DateTimeOffset.UtcNow));

            var updateResult = await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        public async Task AddWithdrawalBankAccountAsync(ObjectId userAccountId, CreateBankAccountDto dto)
        {
            var bankAccountId = await _bankAccountRepository.CreateBankAccountAsync(dto);
            var filterDefinition = Builders<UserAccount>.Filter.Eq(a => a.Id, userAccountId);

            var updateBuilder = new UpdateDefinitionBuilder<UserAccount>();
            var updateDefinition =
                updateBuilder.Combine(updateBuilder.Set(a => a.WithdrawalBankAccountId, bankAccountId),
                    updateBuilder.Set(a => a.DateModified, DateTimeOffset.UtcNow));

            var updateResult = await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        public async Task<UserAccountDto?> GetAccountAsync(ObjectId id)
        {
            var accountCursor = await _collection.FindAsync(a => a.Id == id);
            return await GetAccountAsync(accountCursor);
        }

        public async Task<UserAccountDto?> GetAccountAsync(string walletAddress)
        {
            var accountCursor = await _collection.FindAsync(a => a.WalletAddress == walletAddress);
            return await GetAccountAsync(accountCursor);
        }

        public async Task<Guid> GetNonceAsync(string walletAddress)
        {
            var projection = Builders<UserAccount>.Projection.Expression(account => account.Nonce);
            var filter = Builders<UserAccount>.Filter.Eq(account => account.WalletAddress, walletAddress);
            var result = await _collection.FindAsync(filter, new FindOptions<UserAccount, Guid>
            {
                Projection = projection
            });

            return result == null
                ? Guid.Empty
                : await result.FirstOrDefaultAsync();
        }

        public async Task GenerateNewNonceAsync(ObjectId id)
        {
            var filterDefinition = Builders<UserAccount>.Filter.Eq(a => a.Id, id);
            var updateBuilder = new UpdateDefinitionBuilder<UserAccount>();

            var updateDefinition = updateBuilder.Set(a => a.Nonce, Guid.NewGuid());

            var updateResult = await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        public async Task UpdateUserAccountAsync(ObjectId id, CreateAccountDto dto)
        {
            if (dto.FirstName == null)
            {
                throw new ValidationException("First name is required");
            }

            if (dto.LastName == null)
            {
                throw new ValidationException("Last name is required");
            }

            var filterDefinition = Builders<UserAccount>.Filter.Eq(a => a.Id, id);
            var updateBuilder = new UpdateDefinitionBuilder<UserAccount>();
            var updateDefinition = updateBuilder.Combine(updateBuilder.Set(a => a.FirstName, dto.FirstName),
                updateBuilder.Set(a => a.LastName, dto.LastName),
                updateBuilder.Set(a => a.DateModified, DateTimeOffset.UtcNow));

            var updateResult = await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        private async Task<UserAccountDto?> GetAccountAsync(IAsyncCursor<UserAccount>? cursor)
        {
            var accountTask = cursor?.FirstOrDefaultAsync();
            var account = accountTask != null ? await accountTask : null;

            if (account == null)
            {
                return null;
            }

            var dto = new UserAccountDto
            {
                Id            = account.Id,
                FirstName     = account.FirstName,
                LastName      = account.LastName,
                WalletAddress = account.WalletAddress,
                Nonce         = account.Nonce,
                DepositAccount = account.DepositBankAccountId == null
                    ? null
                    : await _bankAccountRepository.GetBankAccountAsync(account.DepositBankAccountId.Value),
                WithdrawalAccount = account.WithdrawalBankAccountId == null
                    ? null
                    : await _bankAccountRepository.GetBankAccountAsync(account.WithdrawalBankAccountId.Value)
            };

            return dto;
        }

        private async Task<UserAccount?> GetUserAccountEntityAsync(ObjectId id)
        {
            var cursor = await _collection.FindAsync(a => a.Id == id);
            var accountTask = cursor?.FirstOrDefaultAsync();
            return accountTask != null ? await accountTask : null;
        }
    }
}