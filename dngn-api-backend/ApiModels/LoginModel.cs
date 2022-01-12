using System.ComponentModel.DataAnnotations;

namespace DngnApiBackend.ApiModels
{
    public class LoginModel
    {
        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string Signature { get; set; } = null!;
    }
}