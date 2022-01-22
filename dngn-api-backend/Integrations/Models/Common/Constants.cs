namespace DngnApiBackend.Integrations.Models.Common
{
    public static class Constants
    {
        public const string CurrencyCode = "NGN";

        // hard-coded. only one virtual account provider supported.
        public const Common.VirtualAccountProvider VirtualAccountProvider = Common.VirtualAccountProvider.Flutterwave;

        public const string VirtualAccountReferencePrefix = "DNGN-DEPOSITACCT";
    }
}