using System.Collections.Generic;

namespace DngnApiBackend.Data.Models
{
    public class Bank : BaseModel
    {
        public string Name { get; set; } = null!;
        public string NormalizedName { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<BankMetaKey, string> Metadata { get; set; } =
            new Dictionary<BankMetaKey, string>();
    }
}