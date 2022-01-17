namespace DngnApiBackend.Services.Platform.Pricing
{
    public interface IPriceCalculatorService
    {
        long CalculateDepositFees(long amount);

        long CalculateWithdrawalFees(long amount);
    }
}