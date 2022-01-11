using System.ComponentModel.DataAnnotations;

namespace DngnApiBackend.ApiModels
{
    public class LoginModel
    {
        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Signature { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string SignedData { get; set; } = null!;

        [Required]
        public string Signature { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;
    }
}