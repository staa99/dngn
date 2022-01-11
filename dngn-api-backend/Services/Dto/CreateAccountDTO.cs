namespace DngnApiBackend.Services.Dto
{
    public class CreateAccountDto
    {
        public string? WalletAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}