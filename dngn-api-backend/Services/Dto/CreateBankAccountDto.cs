using System.Collections.Generic;
using System.Text.Json.Serialization;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Utilities;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class CreateBankAccountDto
    {
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public ObjectId? BankId { get; set; }
        public ObjectId UserId { get; set; }
        public bool IsVirtual { get; set; }

        [JsonConverter(typeof(DictionaryTKeyEnumTValueConverter))]
        public IDictionary<BankAccountMetaKey, string> Metadata { get; set; } =
            new Dictionary<BankAccountMetaKey, string>();
    }
}