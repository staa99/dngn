using System;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class JwtGenerationDto
    {
        public ObjectId Id { get; set; }
        public string Address { get; set; } = null!;
    }

    public class UserAccountDto
    {
        public ObjectId Id { get; set; }
        public Guid Nonce { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string WalletAddress { get; set; } = null!;
        public BankAccountDto? DepositAccount { get; set; }
        public BankAccountDto? WithdrawalAccount { get; set; }
    }
}