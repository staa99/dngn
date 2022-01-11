﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class UserAccountRepository : BaseRepository<UserAccount>, IUserAccountRepository
    {
        private readonly IBankAccountRepository _bankAccountRepository;

        public UserAccountRepository(IMongoDatabase database, IBankAccountRepository bankAccountRepository) : base(
            database, DngnMongoSchema.UserAccountCollection)
        {
            _bankAccountRepository = bankAccountRepository;
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

            var updateDefinition = BuildUpdate(new List<UpdateDefinition<UserAccount>>
            {
                UpdateBuilder.Set(a => a.DepositBankAccountId, bankAccountId)
            });

            var updateResult = await _collection.UpdateOneAsync(FilterById(userAccountId), updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        public async Task AddWithdrawalBankAccountAsync(ObjectId userAccountId, CreateBankAccountDto dto)
        {
            var bankAccountId = await _bankAccountRepository.CreateBankAccountAsync(dto);

            var updateDefinition = BuildUpdate(new List<UpdateDefinition<UserAccount>>
            {
                UpdateBuilder.Set(a => a.WithdrawalBankAccountId, bankAccountId)
            });

            var updateResult = await _collection.UpdateOneAsync(FilterById(userAccountId), updateDefinition);
            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new ValidationException("User not found");
            }
        }

        public async Task<UserAccountDto?> GetAccountAsync(ObjectId id)
        {
            var accountCursor = await _collection.FindAsync(FilterById(id));
            return await GetAccountAsync(accountCursor);
        }

        public async Task<UserAccountDto?> GetAccountAsync(string walletAddress)
        {
            var accountCursor = await _collection.FindAsync(FilterByWalletAddress(walletAddress));
            return await GetAccountAsync(accountCursor);
        }

        public async Task<Guid> GetNonceAsync(string walletAddress)
        {
            var result = await _collection.FindAsync(FilterByWalletAddress(walletAddress),
                new FindOptions<UserAccount, Guid>
                {
                    Projection = Project(account => account.Nonce)
                });

            return result == null
                ? Guid.Empty
                : await result.FirstOrDefaultAsync();
        }

        public async Task GenerateNewNonceAsync(ObjectId id)
        {
            var updateDefinition = BuildUpdate(new List<UpdateDefinition<UserAccount>>
            {
                UpdateBuilder.Set(a => a.Nonce, Guid.NewGuid())
            });
            var updateResult = await _collection.UpdateOneAsync(FilterById(id), updateDefinition);

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

            var updateDefinition = BuildUpdate(new List<UpdateDefinition<UserAccount>>
            {
                UpdateBuilder.Set(a => a.FirstName, dto.FirstName),
                UpdateBuilder.Set(a => a.LastName, dto.LastName)
            });

            var updateResult = await _collection.UpdateOneAsync(FilterById(id), updateDefinition);
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

        private static FilterDefinition<UserAccount> FilterByWalletAddress(string walletAddress)
        {
            return Builders<UserAccount>.Filter.Eq(account => account.WalletAddress, walletAddress.ToLowerInvariant());
        }
    }
}