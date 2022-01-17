using DngnApiBackend.Configuration;
using Microsoft.Extensions.Options;

namespace DngnApiBackend.Services.Platform.Pricing
{
    public class PriceCalculatorService : IPriceCalculatorService
    {
        private readonly DngnOptions _options;


        public PriceCalculatorService(IOptions<DngnOptions> options)
        {
            _options = options.Value;
        }


        public long CalculateDepositFees(long amount)
        {
            return CalculatePercentageLimitFees(amount, _options.CreditChargePercentage, _options.CreditChargeLimit);
        }


        public long CalculateWithdrawalFees(long amount)
        {
            return CalculatePercentageLimitFees(amount, _options.DebitChargePercentage, _options.DebitChargeLimit);
        }


        private static long CalculatePercentageLimitFees(long amount, decimal percentage, long flatLimit)
        {
            var fees = percentage * amount / 100;
            if (fees > flatLimit)
            {
                fees = flatLimit;
            }

            return (long) fees;
        }
    }
}