using DngnApiBackend.Integrations.Models.Common;

namespace DngnApiBackend.Integrations.Models.CreateVirtualAccount
{
    public class CreateVirtualAccountInput
    {
        public string? BVN { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public VirtualAccountProvider Provider { get; set; }
    }
}