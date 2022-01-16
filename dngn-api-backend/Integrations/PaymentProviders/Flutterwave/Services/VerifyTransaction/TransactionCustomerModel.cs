using System;
using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.VerifyTransaction
{
    public class TransactionCustomerModel
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("name")] public string? Name { get; set; }

        [JsonPropertyName("phone_number")] public string? PhoneNumber { get; set; }

        [JsonPropertyName("email")] public string? Email { get; set; }

        [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    }
}