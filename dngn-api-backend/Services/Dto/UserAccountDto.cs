using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class UserAccountDto
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        [JsonPropertyName("id")]
        public string IdString => Id.ToString();

        public Guid Nonce { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string WalletAddress { get; set; } = null!;
        public BankAccountDto? DepositAccount { get; set; }
        public BankAccountDto? WithdrawalAccount { get; set; }
    }
}