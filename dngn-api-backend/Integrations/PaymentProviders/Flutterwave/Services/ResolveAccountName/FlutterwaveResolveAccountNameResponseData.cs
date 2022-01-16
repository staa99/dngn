using System.Text.Json.Serialization;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.ResolveAccountName
{
    public class FlutterwaveResolveAccountNameResponseData : IFlutterwaveResponseData
    {
        [JsonPropertyName("account_number")] public string? AccountNumber { get; set; }

        [JsonPropertyName("account_name")] public string? AccountName { get; set; }
    }
}