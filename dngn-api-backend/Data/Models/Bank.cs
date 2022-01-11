using System.Collections.Generic;

namespace DngnApiBackend.Data.Models
{
    public class Bank : BaseModel
    {
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}