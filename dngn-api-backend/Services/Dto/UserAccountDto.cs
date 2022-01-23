using System;

namespace DngnApiBackend.Services.Dto
{
    public class UserAccountDto: BaseIdDto
    {
        public Guid Nonce { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string WalletAddress { get; set; } = null!;
        public BankAccountDto? DepositAccount { get; set; }
        public BankAccountDto? WithdrawalAccount { get; set; }
    }
}