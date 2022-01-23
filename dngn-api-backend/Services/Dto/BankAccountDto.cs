using System.Collections.Generic;
using System.Text.Json.Serialization;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class BankAccountDto: BaseIdDto
    {
        [JsonIgnore]
        public ObjectId UserId { get; set; }

        [JsonPropertyName("userId")]
        public string UserIdString => UserId.ToString();
        public string AccountNumber { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string? BankName { get; set; }
        public bool IsVirtual { get; set; }
        public BankDto? Bank { get; set; }

        public IDictionary<BankAccountMetaKey, string> Metadata { get; set; } =
            new Dictionary<BankAccountMetaKey, string>();
    }
}