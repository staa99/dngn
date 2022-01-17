using System.Collections.Generic;
using System.Text.Json.Serialization;
using DngnApiBackend.Utilities;

namespace DngnApiBackend.Integrations.BankUtilities
{
    public class BankListServiceResponseItem
    {
        /// <summary>
        ///     Maps bank code types of each response item to the actual bank codes
        /// </summary>
        [JsonConverter(typeof(DictionaryTKeyEnumTValueConverter))]
        public IDictionary<BankCodeType, string> Codes { get; set; } = new Dictionary<BankCodeType, string>();

        /// <summary>
        ///     The name of the bank
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        ///     The URL of the bank's logo
        /// </summary>
        public string? LogoUrl { get; set; }
    }
}