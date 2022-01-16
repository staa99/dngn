using System.Text.Json.Serialization;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.CreateVirtualAccount
{
    public class FlutterwaveCreateVirtualAccountRequest
    {
        public FlutterwaveCreateVirtualAccountRequest(string email,
            string fullName,
            string bvn,
            string transactionReference)
        {
            Email                = email;
            FullName             = fullName;
            BVN                  = bvn;
            TransactionReference = transactionReference;
        }


        [JsonPropertyName("email")] public string Email { get; }

        [JsonPropertyName("narration")] public string FullName { get; }

        [JsonPropertyName("bvn")] public string BVN { get; }

        [JsonPropertyName("tx_ref")] public string TransactionReference { get; }

        [JsonPropertyName("is_permanent")]
        public bool IsPermanent => true; // all virtual accounts created are permanent
    }
}