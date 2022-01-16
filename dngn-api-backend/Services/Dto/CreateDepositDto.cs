namespace DngnApiBackend.Services.Dto
{
    public class CreateDepositDto: CreateTransactionDto
    {
        public string? RawWebhookPayload { get; set; }
    }
}