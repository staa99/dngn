using System.Collections.Generic;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Abstractions;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.GetBanks
{
    public class FlutterwaveGetBanksResponseData : List<FlutterwaveBank>, IFlutterwaveResponseData
    {
    }
}