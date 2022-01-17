using System.Collections.Generic;
using DngnApiBackend.Data.Models;
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

        public IDictionary<BankAccountMetaKey, string> Metadata { get; set; } =
            new Dictionary<BankAccountMetaKey, string>();
    }
}