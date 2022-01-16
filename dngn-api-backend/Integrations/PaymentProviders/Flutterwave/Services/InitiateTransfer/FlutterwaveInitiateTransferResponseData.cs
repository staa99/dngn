using System.Text.Json.Serialization;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

#pragma warning disable 649


namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.InitiateTransfer
{
    public class FlutterwaveInitiateTransferResponseData : IFlutterwaveResponseData
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("account_number")] public string? AccountNumber { get; set; }

        [JsonPropertyName("bank_code")] public string? BankCode { get; set; }

        [JsonPropertyName("full_name")] public string? FullName { get; set; }

        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }

        [JsonPropertyName("currency")] public string? Currency { get; set; }

        [JsonPropertyName("debit_currency")] public string? DebitCurrency { get; set; }

        [JsonPropertyName("amount")] public decimal Amount { get; set; }

        [JsonPropertyName("fee")] public decimal Fee { get; set; }

        [JsonPropertyName("status")] public string? Status { get; set; }

        [JsonPropertyName("reference")] public string? Reference { get; set; }

        [JsonPropertyName("meta")] public string? Meta { get; set; }

        [JsonPropertyName("narration")] public string? Narration { get; set; }

        [JsonPropertyName("complete_message")] public string? CompleteMessage { get; set; }

        [JsonPropertyName("requires_approval")]
        public byte RequiresApproval { get; set; }

        [JsonPropertyName("is_approved")] public byte IsApproved { get; set; }

        [JsonPropertyName("bank_name")] public string? BankName { get; set; }
    }
}