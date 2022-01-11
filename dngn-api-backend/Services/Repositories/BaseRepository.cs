using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DngnApiBackend.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class BaseRepository<T> where T : BaseModel
    {
        protected readonly IMongoCollection<T> _collection;
        
        public BaseRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }


        protected static UpdateDefinitionBuilder<T> UpdateBuilder => Builders<T>.Update;

        protected static FilterDefinition<T> FilterById(ObjectId id)
        {
            return Builders<T>.Filter.Eq(m => m.Id, id);
        }

        protected static UpdateDefinition<T> BuildUpdate(List<UpdateDefinition<T>> updates)
        {
            // set date modified
            updates.Add(UpdateBuilder.Set(m => m.DateModified, DateTimeOffset.UtcNow));
            return UpdateBuilder.Combine(updates);
        }

        protected static ProjectionDefinition<T, TValue> Project<TValue>(Expression<Func<T, TValue>> projection)
        {
            return Builders<T>.Projection.Expression(projection);
        }
    }
}