using System;
using MongoDB.Bson;

namespace DngnApiBackend.Data.Models
{
    public abstract class BaseTransactionModel : BaseModel
    {
        public string? BankTransactionId { get; set; }
        public string ProviderTransactionId { get; set; } = null!;
        public Guid InternalTransactionId { get; set; }
        public long Amount { get; set; }
        public long ProviderFees { get; set; }
        public long TotalPlatformFees { get; set; }
        public TransactionStatus Status { get; set; }
        public ObjectId UserAccountId { get; set; }
        public ObjectId BankAccountId { get; set; }
    }
}