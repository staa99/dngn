using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.Notifications.Outgoing
{
    public class MinterInstruction
    {
        public MinterInstruction(string providerTransactionReference, string toAddress, long amount, long fees)
        {
            ProviderTransactionReference = providerTransactionReference;
            ToAddress                    = toAddress;
            Amount                       = amount;
            Fees                         = fees;
        }

        [JsonPropertyName("offChainTransactionId")]
        public string ProviderTransactionReference { get; }

        [JsonPropertyName("to")]
        public string ToAddress { get; }

        [JsonPropertyName("amount")]
        public long Amount { get; }

        [JsonPropertyName("fees")]
        public long Fees { get; }
    }
}