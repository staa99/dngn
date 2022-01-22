using System.ComponentModel.DataAnnotations;

namespace DngnApiBackend.ApiModels
{
    public class GenerateDepositAccountModel
    {
        [Required]
        public string EmailAddress { get; set; } = null!;
        
        [Required]
        public string BVN { get; set; } = null!;
    }
}