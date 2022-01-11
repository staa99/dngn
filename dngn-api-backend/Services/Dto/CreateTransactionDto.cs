using System;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class CreateTransactionDto
    {
        public string? BankTransactionId { get; set; }
        public string? ProviderTransactionId { get; set; }
        public Guid InternalTransactionId { get; set; }
        public long Amount { get; set; }
        public long ProviderFees { get; set; }
        public long TotalPlatformFees { get; set; }
        public TransactionStatus Status { get; set; }
        public ObjectId UserAccountId { get; set; }
    }
}