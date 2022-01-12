using System.ComponentModel.DataAnnotations;

namespace DngnApiBackend.ApiModels
{
    public class GenerateDepositAccountModel
    {
        [Required]
        public string BVN { get; set; } = null!;
    }
}