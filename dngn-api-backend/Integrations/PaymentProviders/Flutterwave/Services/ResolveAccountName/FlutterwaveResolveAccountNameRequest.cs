using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.ResolveAccountName
{
    public class FlutterwaveResolveAccountNameRequest
    {
        [JsonPropertyName("account_number")] public string? AccountNumber { get; set; }

        [JsonPropertyName("account_bank")] public string? BankCode { get; set; }
    }
}