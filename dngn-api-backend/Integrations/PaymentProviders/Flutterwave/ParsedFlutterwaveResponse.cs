using System.Text.Json;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave
{
    public class ParsedFlutterwaveResponse<TResponseData> where TResponseData : class, IFlutterwaveResponseData, new()
    {
        public ParsedFlutterwaveResponse(FlutterwaveResponse flutterwaveResponse)
        {
            Status  = flutterwaveResponse.Status;
            Message = flutterwaveResponse.Message;
            Data    = JsonSerializer.Deserialize<TResponseData>(flutterwaveResponse.RawData);
        }


        public bool IsSuccessful => Status.Trim().ToLowerInvariant() == "success";
        public string Status { get; }
        public string Message { get; }
        public TResponseData? Data { get; }
    }
}