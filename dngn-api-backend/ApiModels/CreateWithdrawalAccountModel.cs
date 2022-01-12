using System.ComponentModel.DataAnnotations;

namespace DngnApiBackend.ApiModels
{
    public class CreateWithdrawalAccountModel
    {
        [Required]
        public string AccountNumber { get; set; } = null!;
        
        [Required]
        public string AccountName { get; set; } = null!;
        
        [Required]
        public string BankId { get; set; } = null!;
    }
}