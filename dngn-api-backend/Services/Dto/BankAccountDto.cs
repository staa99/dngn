using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class BankAccountDto
    {
        public ObjectId Id { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string? BankName { get; set; }
        public bool IsVirtual { get; set; }
        public BankDto? Bank { get; set; }
    }
}