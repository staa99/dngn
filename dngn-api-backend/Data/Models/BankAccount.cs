using System.Collections.Generic;
using MongoDB.Bson;

namespace DngnApiBackend.Data.Models
{
    public class BankAccount : BaseModel
    {
        public string AccountNumber { get; set; } = null!;
        public ObjectId? BankId { get; set; }
        public string AccountName { get; set; } = null!;
        public bool IsVirtual { get; set; }
        public IDictionary<BankAccountMetaKey, string> Metadata { get; set; } = new Dictionary<BankAccountMetaKey, string>();
    }
}