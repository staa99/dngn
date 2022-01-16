using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.InitiateTransfer
{
    public class FlutterwaveInitiateTransferRequest
    {
        public FlutterwaveInitiateTransferRequest(string? accountBank,
            string? accountNumber,
            decimal amount,
            string? narration,
            string? currency,
            string? reference)
        {
            AccountBank   = accountBank;
            AccountNumber = accountNumber;
            Amount        = amount;
            Narration     = narration;
            Currency      = currency;
            Reference     = reference;
        }


        [JsonPropertyName("account_bank")] public string? AccountBank { get; set; }

        [JsonPropertyName("account_number")] public string? AccountNumber { get; }

        [JsonPropertyName("amount")] public decimal Amount { get; }

        [JsonPropertyName("narration")] public string? Narration { get; }

        [JsonPropertyName("currency")] public string? Currency { get; }

        [JsonPropertyName("reference")] public string? Reference { get; }
    }
}