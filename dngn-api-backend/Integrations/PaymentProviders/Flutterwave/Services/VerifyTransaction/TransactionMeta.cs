using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.VerifyTransaction
{
    public class TransactionMeta
    {
        [JsonPropertyName("originatoraccountnumber")]
        public string? OriginatorAccountNumber { get; set; }

        [JsonPropertyName("originatorname")] public string? OriginatorName { get; set; }

        [JsonPropertyName("bankname")] public string? BankName { get; set; }

        [JsonPropertyName("originatoramount")] public string? OriginatorAmount { get; set; }
    }
}