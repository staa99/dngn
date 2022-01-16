namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave
{
    public class FlutterwaveOptions
    {
        public string BaseUri { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string SecretHash { get; set; } = null!;
    }
}