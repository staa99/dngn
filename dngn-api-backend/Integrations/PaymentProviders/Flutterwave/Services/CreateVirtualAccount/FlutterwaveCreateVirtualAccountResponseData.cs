using System.Text.Json.Serialization;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

#pragma warning disable 649


namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.CreateVirtualAccount
{
    public class FlutterwaveCreateVirtualAccountResponseData : IFlutterwaveResponseData
    {
        [JsonPropertyName("response_code")] public string? ResponseCode { get; set; }

        [JsonPropertyName("response_message")] public string? ResponseMessage { get; set; }

        [JsonPropertyName("flw_ref")] public string? FlwReference { get; set; }

        [JsonPropertyName("order_ref")] public string? OrderReference { get; set; }

        [JsonPropertyName("account_number")] public string? AccountNumber { get; set; }

        [JsonPropertyName("bank_name")] public string? BankName { get; set; }

        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }

        [JsonPropertyName("expiry_date")] public string? ExpiryDate { get; set; }

        [JsonPropertyName("note")] public string? Note { get; set; }

        [JsonPropertyName("amount")] public string? Amount { get; set; }
    }
}