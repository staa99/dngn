using DngnApiBackend.Data.Models;

namespace DngnApiBackend.Integrations.Models.Common
{
    public abstract class BaseTransferOutput
    {
        public string? StatusMessage { get; set; }
        public TransactionStatus Status { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal TransferFees { get; set; }
        public string? ProviderTransactionReference { get; set; }
        public string? ProviderBankReference { get; set; }
    }
}