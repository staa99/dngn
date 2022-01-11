using System.Collections.Generic;

namespace DngnApiBackend.Services.Dto
{
    public class CreateBankDto
    {
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}