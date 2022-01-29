using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.Blockchain.Incoming
{
    public class BlockchainIncomingInstruction
    {
        [JsonPropertyName("txHash")]
        public string TransactionHash { get; set; } = null!;

        [JsonPropertyName("address")]
        public string ToAddress { get; set; } = null!;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}