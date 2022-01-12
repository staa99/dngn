using MongoDB.Bson;

namespace DngnApiBackend.Services.Dto
{
    public class JwtGenerationDto
    {
        public ObjectId Id { get; set; }
        public string Address { get; set; } = null!;
    }
}