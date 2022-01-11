namespace DngnApiBackend.Data.Models
{
    public class Deposit : BaseTransactionModel
    {
        public string RawWebhookPayload { get; set; } = null!;
    }
}