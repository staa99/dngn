using System.Collections.Generic;

namespace DngnApiBackend.Integrations.BankUtilities
{
    public class BankListServiceResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public ICollection<BankListServiceResponseItem> Data { get; set; } = new List<BankListServiceResponseItem>();
    }
}