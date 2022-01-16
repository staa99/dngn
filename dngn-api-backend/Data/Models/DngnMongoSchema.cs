using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Data.Models
{
    public static class DngnMongoSchema
    {
        public const string UserAccountCollection = "user_accounts";
        public const string BankAccountCollection = "bank_accounts";
        public const string BankCollection = "banks";
        public const string DepositCollection = "deposits";
        public const string WithdrawalCollection = "withdrawals";

        public static async Task RegisterDngnIndexes(this IMongoDatabase db)
        {
            await AddIndexAsync(db, UserAccountCollection,
                Builders<UserAccount>.IndexKeys.Ascending(a => a.WalletAddress));

            await AddIndicesAsync(db, BankAccountCollection, new[]
            {
                Builders<BankAccount>.IndexKeys.Ascending(a => a.BankId),
                Builders<BankAccount>.IndexKeys.Ascending(a => a.AccountNumber)
            });

            await AddIndicesAsync(db, BankCollection, new[]
            {
                Builders<Bank>.IndexKeys.Ascending(b => b.CBNCode),
                Builders<Bank>.IndexKeys.Ascending(b => b.NIPCode),
                Builders<Bank>.IndexKeys.Ascending(b => b.ShortName),
                Builders<Bank>.IndexKeys.Text(b => b.Name)
            });

            await AddIndicesAsync(db, DepositCollection, new[]
            {
                Builders<Deposit>.IndexKeys.Ascending(d => d.UserAccountId),
                Builders<Deposit>.IndexKeys.Ascending(d => d.BankAccountId),
                Builders<Deposit>.IndexKeys.Ascending(d => d.Amount)
            });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.BankTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.InternalTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.ProviderTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });

            await AddIndicesAsync(db, WithdrawalCollection, new[]
            {
                Builders<Withdrawal>.IndexKeys.Ascending(w => w.UserAccountId),
                Builders<Withdrawal>.IndexKeys.Ascending(w => w.BankAccountId),
                Builders<Withdrawal>.IndexKeys.Ascending(w => w.Amount)
            });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.BankTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.InternalTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.ProviderTransactionId), new CreateIndexOptions
                {
                    Unique = true
                });
        }

        private static async Task AddIndicesAsync<T>(IMongoDatabase db, string collectionName,
            IEnumerable<IndexKeysDefinition<T>> definitions)
        {
            await EnsureCollectionAsync<T>(db, collectionName);
            var collection = db.GetCollection<T>(collectionName);

            await collection.Indexes.CreateManyAsync(definitions.Select(definition =>
                new CreateIndexModel<T>(definition)));
        }

        private static async Task AddIndexAsync<T>(IMongoDatabase db, string collectionName,
            IndexKeysDefinition<T> definition, CreateIndexOptions? options = null)
        {
            await EnsureCollectionAsync<T>(db, collectionName);
            var collection = db.GetCollection<T>(collectionName);

            await collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(definition, options));
        }

        private static async Task EnsureCollectionAsync<T>(IMongoDatabase db, string name)
        {
            var filter = new BsonDocument("name", name);
            var collections = await db.ListCollectionsAsync(new ListCollectionsOptions {Filter = filter});
            var exists = await collections.AnyAsync();

            if (exists)
            {
                return;
            }

            await db.CreateCollectionAsync(name);
        }
    }
}