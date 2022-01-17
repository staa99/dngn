using System;
using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Repositories
{
    public interface ITransactionRepository
    {
        Task<ObjectId> CreateTransactionAsync<TDto>(TDto dto) where TDto : CreateTransactionDto;
        Task<TransactionDto?> GetTransactionAsync(ObjectId id);
        Task<TransactionDto?> GetTransactionByBankReferenceAsync(string bankReference);

        Task<TransactionDto?> GetTransactionByProviderReferenceAsync(TransactionProvider provider,
            string providerReference);

        Task<TransactionDto?> GetTransactionByReferenceAsync(Guid transactionReference);
    }
}