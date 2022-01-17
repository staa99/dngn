using DngnApiBackend.Integrations.Models.Common;

namespace DngnApiBackend.Integrations.Models.LogVirtualAccountTransaction
{
    public class LogVirtualAccountTransactionInput
    {
        public bool IsSuccessful => Status.Trim().ToLowerInvariant() == "successful";
        public string ProviderTransactionReference { get; set; } = null!;
        public string? BankTransactionReference { get; set; }
        public string ProviderVirtualAccountId { get; set; } = null!;
        public string? Currency { get; set; }
        public VirtualAccountProvider Provider { get; set; }
        public long Amount { get; set; }
        public long Fees { get; set; }
        public string? Narration { get; set; }
        public string Status { get; set; } = null!;
    }
}