using System.Collections.Generic;
using System.Threading.Tasks;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface IBankRepository
    {
        Task<ObjectId> CreateBankAsync(CreateBankDto dto);
        Task<BankDto?> GetBankAsync(ObjectId id);
        Task UpdateBankAsync(ObjectId id, CreateBankDto dto);
        Task<ICollection<BankDto>> GetBanksAsync(string query);
        Task<BankDto?> GetBanksNIPAsync(string nipCode);
        Task<BankDto?> GetBanksCBNAsync(string cbnCode);
    }
}