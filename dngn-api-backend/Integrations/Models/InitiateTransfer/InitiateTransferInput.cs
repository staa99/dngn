using DngnApiBackend.Integrations.Models.Common;

namespace DngnApiBackend.Integrations.Models.InitiateTransfer
{
    public class InitiateTransferInput
    {
        public string? ProviderBankCode { get; set; }
        public TransactionProvider Provider { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionReference { get; set; }
        public string? Narration { get; set; }
        public string? Currency { get; set; }
    }
}