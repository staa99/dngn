using System;
using System.Text.Json.Serialization;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.VerifyTransaction
{
    public class FlutterwaveVerifyTransactionResponseData : IFlutterwaveResponseData
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("tx_ref")] public string? TxRef { get; set; }

        [JsonPropertyName("flw_ref")] public string? FlwRef { get; set; }

        [JsonPropertyName("device_fingerprint")]
        public string? DeviceFingerprint { get; set; }

        [JsonPropertyName("amount")] public decimal Amount { get; set; }

        [JsonPropertyName("currency")] public string? Currency { get; set; }

        [JsonPropertyName("charged_amount")] public decimal ChargedAmount { get; set; }

        [JsonPropertyName("app_fee")] public decimal AppFee { get; set; }

        [JsonPropertyName("merchant_fee")] public decimal MerchantFee { get; set; }

        [JsonPropertyName("processor_response")]
        public string? ProcessorResponse { get; set; }

        [JsonPropertyName("auth_model")] public string? AuthModel { get; set; }

        [JsonPropertyName("ip")] public string? Ip { get; set; }

        [JsonPropertyName("narration")] public string? Narration { get; set; }

        [JsonPropertyName("status")] public string? Status { get; set; }

        [JsonPropertyName("payment_type")] public string? PaymentType { get; set; }

        [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("account_id")] public long AccountId { get; set; }

        [JsonPropertyName("meta")] public TransactionMeta? Meta { get; set; }

        [JsonPropertyName("amount_settled")] public decimal AmountSettled { get; set; }

        [JsonPropertyName("customer")] public TransactionCustomerModel? Customer { get; set; }
    }
}