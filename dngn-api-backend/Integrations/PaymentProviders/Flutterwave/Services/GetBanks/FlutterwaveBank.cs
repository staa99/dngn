using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.GetBanks
{
    public class FlutterwaveBank
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("code")] public string Code { get; set; } = null!;

        [JsonPropertyName("name")] public string Name { get; set; } = null!;
    }
}