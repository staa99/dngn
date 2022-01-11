using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DngnApiBackend.Data.Extensions
{
    public static class MongoDbConnectionExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            return serviceCollection.AddSingleton<IMongoDatabase>(_ =>
            {
                var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("Mongo"));
                settings.LinqProvider = LinqProvider.V3;
                var client = new MongoClient(settings);
                var db = client.GetDatabase(configuration["DBName"]);
                return db;
            });
        }
    }
}