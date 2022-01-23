using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class BaseIdDto
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        [JsonPropertyName("id")]
        public string IdString => Id.ToString();
    }
}