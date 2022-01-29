using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DngnApiBackend.Data.Seeds
{
    public class BankSeedModel
    {
        [JsonPropertyName("banks")]
        public ICollection<BankSeedItemModel> Banks { get; set; } = new List<BankSeedItemModel>();
    }
}