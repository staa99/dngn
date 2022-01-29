using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
            BsonSerializer.RegisterSerializer(new EnumSerializer<BankAccountMetaKey>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<BankMetaKey>(BsonType.String));
            await AddIndexAsync(db, UserAccountCollection,
                Builders<UserAccount>.IndexKeys.Ascending(a => a.WalletAddress), new CreateIndexOptions
                {
                    Name = "IX_UserAccount_WalletAddress"
                });

            await AddIndicesAsync(db, BankAccountCollection, new Dictionary<string,IndexKeysDefinition<BankAccount>>()
            {
                ["IX_BankAccount_BankId"] = Builders<BankAccount>.IndexKeys.Ascending(a => a.BankId),
                ["IX_BankAccount_AccountNumber"] = Builders<BankAccount>.IndexKeys.Ascending(a => a.AccountNumber)
            });

            await AddIndexAsync(db, BankAccountCollection,
                Builders<BankAccount>.IndexKeys.Combine(
                    Builders<BankAccount>.IndexKeys.Ascending(new StringFieldDefinition<BankAccount>(
                        $"{nameof(BankAccount.Metadata)}.{nameof(BankAccountMetaKey.Provider)}")),
                    Builders<BankAccount>.IndexKeys.Ascending(new StringFieldDefinition<BankAccount>(
                        $"{nameof(BankAccount.Metadata)}.{nameof(BankAccountMetaKey.ProviderAccountReference)}"))),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UQ_BankAccount_ProviderReference"
                });

            await AddIndicesAsync(db, BankCollection, new Dictionary<string,IndexKeysDefinition<Bank>>()
            {
                ["IX_Bank_CBNCode"] = Builders<Bank>.IndexKeys.Ascending(b => b.CBNCode),
                ["IX_Bank_NIPCode"] = Builders<Bank>.IndexKeys.Ascending(b => b.NIPCode),
                ["IX_Bank_ShortName"] = Builders<Bank>.IndexKeys.Ascending(b => b.ShortName),
                ["IX_TXT_Bank_Name"] = Builders<Bank>.IndexKeys.Text(b => b.Name)
            });

            await AddIndicesAsync(db, DepositCollection, new Dictionary<string,IndexKeysDefinition<Deposit>>()
            {
                ["IX_Deposit_UserAccountId"] = Builders<Deposit>.IndexKeys.Ascending(d => d.UserAccountId),
                ["IX_Deposit_BankAccountId"] = Builders<Deposit>.IndexKeys.Ascending(d => d.BankAccountId),
                ["IX_Deposit_Amount"] = Builders<Deposit>.IndexKeys.Ascending(d => d.Amount)
            });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.BankTransactionId), new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UQ_Deposit_BankTransactionId"
                });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.InternalTransactionId), new CreateIndexOptions
                {
                    Unique = true,
                    Name   = "UQ_Deposit_InternalTransactionId"
                });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Ascending(d => d.BlockchainTransactionHash), new CreateIndexOptions<Deposit>
                {
                    Unique                  = true,
                    Name                    = "UQ_Deposit_BlockchainTransactionHash",
                    PartialFilterExpression = Builders<Deposit>.Filter.Type(w => w.BlockchainTransactionHash, BsonType.String)
                });

            await AddIndexAsync(db, DepositCollection,
                Builders<Deposit>.IndexKeys.Combine(Builders<Deposit>.IndexKeys.Ascending(d => d.Provider),
                    Builders<Deposit>.IndexKeys.Ascending(d => d.ProviderTransactionId)), new CreateIndexOptions
                {
                    Unique = true,
                    Name   = "UQ_Deposit_ProviderTransactionId"
                });

            await AddIndicesAsync(db, WithdrawalCollection, new Dictionary<string,IndexKeysDefinition<Withdrawal>>()
            {
                ["IX_Withdrawal_UserAccountId"] = Builders<Withdrawal>.IndexKeys.Ascending(d => d.UserAccountId),
                ["IX_Withdrawal_BankAccountId"] = Builders<Withdrawal>.IndexKeys.Ascending(d => d.BankAccountId),
                ["IX_Withdrawal_Amount"] = Builders<Withdrawal>.IndexKeys.Ascending(d => d.Amount)
            });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.BankTransactionId), new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UQ_Withdrawal_BankTransactionId"
                });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.InternalTransactionId), new CreateIndexOptions
                {
                    Unique = true,
                    Name   = "UQ_Withdrawal_InternalTransactionId"
                });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Ascending(d => d.BlockchainTransactionHash), new CreateIndexOptions<Withdrawal>
                {
                    Unique = true,
                    Name   = "UQ_Withdrawal_BlockchainTransactionHash",
                    PartialFilterExpression = Builders<Withdrawal>.Filter.Type(w => w.BlockchainTransactionHash, BsonType.String)
                });

            await AddIndexAsync(db, WithdrawalCollection,
                Builders<Withdrawal>.IndexKeys.Combine(Builders<Withdrawal>.IndexKeys.Ascending(d => d.Provider),
                    Builders<Withdrawal>.IndexKeys.Ascending(d => d.ProviderTransactionId)), new CreateIndexOptions
                {
                    Unique = true,
                    Name   = "UQ_Withdrawal_ProviderTransactionId"
                });
        }

        private static async Task AddIndicesAsync<T>(IMongoDatabase db, string collectionName,
            IDictionary<string, IndexKeysDefinition<T>> definitions)
        {
            await EnsureCollectionAsync<T>(db, collectionName);
            var collection = db.GetCollection<T>(collectionName);

            await collection.Indexes.CreateManyAsync(definitions.Select(definition =>
                new CreateIndexModel<T>(definition.Value, new CreateIndexOptions
                {
                    Name = definition.Key
                })));
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