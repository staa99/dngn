using System;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Integrations.Models.Common;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class CreateTransactionDto
    {
        public string? BankTransactionId { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? BlockchainTransactionHash { get; set; }
        public Guid InternalTransactionId { get; set; }
        public long Amount { get; set; }
        public long ProviderFees { get; set; }
        public long TotalPlatformFees { get; set; }
        public TransactionProvider Provider { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionBlockchainStatus BlockchainStatus { get; set; }
        public TransactionType TransactionType { get; set; }
        public ObjectId BankAccountId { get; set; }
        public ObjectId UserAccountId { get; set; }
    }
}