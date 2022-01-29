using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Integrations.Models.Common;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface IWithdrawalRepository : ITransactionRepository
    {
        Task UpdateTransactionForTransferAsync(ObjectId transactionId, BaseTransferOutput transferOutput);
    }
}