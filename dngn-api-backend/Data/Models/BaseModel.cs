using System;
using MongoDB.Bson;

namespace DngnApiBackend.Data.Models
{
    public abstract class BaseModel
    {
        public ObjectId Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
    }
}