namespace DngnApiBackend.Integrations.Models.CreateVirtualAccount
{
    public class CreateVirtualAccountOutput
    {
        public string BankName { get; set; } = null!;
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? VirtualAccountId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; } = null!;
    }
}