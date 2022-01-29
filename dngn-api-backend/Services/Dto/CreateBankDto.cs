using System.Collections.Generic;
using DngnApiBackend.Data.Models;

namespace DngnApiBackend.Services.Dto
{
    public class CreateBankDto
    {
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<BankMetaKey, string> Metadata { get; set; } = new Dictionary<BankMetaKey, string>();
    }
}