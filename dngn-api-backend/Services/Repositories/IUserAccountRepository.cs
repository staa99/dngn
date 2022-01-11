using System;
using System.Threading.Tasks;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface IUserAccountRepository
    {
        Task<ObjectId> CreateUserAccountAsync(CreateAccountDto dto);
        Task SetDepositBankAccountAsync(ObjectId userAccountId, CreateBankAccountDto dto);
        Task AddWithdrawalBankAccountAsync(ObjectId userAccountId, CreateBankAccountDto dto);

        Task<UserAccountDto?> GetAccountAsync(ObjectId id);
        Task<UserAccountDto?> GetAccountAsync(string walletAddress);
        Task<Guid> GetNonceAsync(string walletAddress);
        Task GenerateNewNonceAsync(ObjectId id);

        Task UpdateUserAccountAsync(ObjectId id, CreateAccountDto dto);
    }
}