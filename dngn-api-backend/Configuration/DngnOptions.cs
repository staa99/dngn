namespace DngnApiBackend.Configuration
{
    public class DngnOptions
    {
        public decimal CreditChargePercentage { get; set; }
        public long CreditChargeLimit { get; set; }
        public decimal DebitChargePercentage { get; set; }
        public long DebitChargeLimit { get; set; }
    }
}