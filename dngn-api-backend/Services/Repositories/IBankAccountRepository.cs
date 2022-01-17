using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface IBankAccountRepository
    {
        Task<ObjectId> CreateBankAccountAsync(CreateBankAccountDto dto);
        Task<BankAccountDto?> GetBankAccountAsync(ObjectId id);
        Task<BankAccountDto?> GetBankAccountAsync(TransactionProvider provider, string providerReference);
    }
}