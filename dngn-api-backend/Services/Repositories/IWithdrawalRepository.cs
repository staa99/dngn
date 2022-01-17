using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface IWithdrawalRepository : ITransactionRepository
    {
        Task UpdateTransactionStatusAsync(ObjectId transactionId, TransactionStatus status, long providerFees);
    }
}