using DngnApiBackend.Data.Models;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Utilities
{
    public static class TransactionStatusUtilities
    {
        public static TransactionStatus GetTransactionStatus(string? queryStatus, string? innerQueryStatus)
        {
            var normalizedQueryStatus = queryStatus?.ToLowerInvariant();
            var normalizedInnerQueryStatus = innerQueryStatus?.ToLowerInvariant();
            return normalizedQueryStatus switch
            {
                "success" when normalizedInnerQueryStatus == "new"        => TransactionStatus.Processing,
                "success" when normalizedInnerQueryStatus == "successful" => TransactionStatus.Successful,
                _                                                         => TransactionStatus.Failed
            };
        }
    }
}