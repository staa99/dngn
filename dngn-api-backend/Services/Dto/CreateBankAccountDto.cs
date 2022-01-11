using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class CreateBankAccountDto
    {
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public ObjectId BankId { get; set; }
        public bool IsVirtual { get; set; }
    }
}