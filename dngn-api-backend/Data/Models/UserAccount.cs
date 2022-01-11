using System;
using MongoDB.Bson;

namespace DngnApiBackend.Data.Models
{
    public class UserAccount : BaseModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string WalletAddress { get; set; } = null!;
        public Guid Nonce { get; set; }
        public ObjectId? DepositBankAccountId { get; set; }
        public ObjectId? WithdrawalBankAccountId { get; set; }
    }
}