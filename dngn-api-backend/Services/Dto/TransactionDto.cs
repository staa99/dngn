using System;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class TransactionDto: BaseIdDto
    {
        public string? BankTransactionId { get; set; }
        public string ProviderTransactionId { get; set; } = null!;
        public string? BlockchainTransactionHash { get; set; }
        public Guid InternalTransactionId { get; set; }
        public long Amount { get; set; }
        public long ProviderFees { get; set; }
        public long TotalPlatformFees { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionBlockchainStatus BlockchainStatus { get; set; }
        public ObjectId UserAccountId { get; set; }
        public ObjectId BankAccountId { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateCompleted { get; set; }
    }
}