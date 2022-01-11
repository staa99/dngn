using System.Collections.Generic;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class BankDto
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? CBNCode { get; set; }
        public string? NIPCode { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}