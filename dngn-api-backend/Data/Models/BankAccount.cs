using MongoDB.Bson;

namespace DngnApiBackend.Data.Models
{
    public class BankAccount : BaseModel
    {
        public string AccountNumber { get; set; } = null!;
        public ObjectId BankId { get; set; }
        public string AccountName { get; set; } = null!;
        public bool IsVirtual { get; set; }
    }
}