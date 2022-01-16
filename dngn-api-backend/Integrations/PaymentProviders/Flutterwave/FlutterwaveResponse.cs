namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave
{
    public class FlutterwaveResponse
    {
        public FlutterwaveResponse(string status, string message, string? rawData)
        {
            Status  = status;
            Message = message;
            RawData = rawData ?? "{}";
        }


        public string Status { get; }
        public string Message { get; }
        public string RawData { get; }
    }
}