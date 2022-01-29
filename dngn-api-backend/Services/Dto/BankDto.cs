using System.Collections.Generic;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class BankDto: BaseIdDto
    {
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<BankMetaKey, string> Metadata { get; set; } =
            new Dictionary<BankMetaKey, string>();
    }
}