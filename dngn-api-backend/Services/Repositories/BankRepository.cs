using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DngnApiBackend.Services.Repositories
{
    public class BankRepository : IBankRepository
    {
        private readonly IMongoCollection<Bank> _collection;

        public BankRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Bank>(DngnMongoSchema.BankCollection);
        }

        public async Task<ObjectId> CreateBankAsync(CreateBankDto dto)
        {
            if (dto.Name == null)
            {
                throw new ValidationException("Bank name is required");
            }

            var bank = new Bank
            {
                Name        = dto.Name,
                ShortName   = dto.ShortName,
                CBNCode     = dto.CBNCode,
                NIPCode     = dto.NIPCode,
                DateCreated = DateTimeOffset.UtcNow
            };
            await _collection.InsertOneAsync(bank);
            return bank.Id;
        }

        public async Task<BankDto?> GetBankAsync(ObjectId id)
        {
            var cursor = await _collection.FindAsync(a => a.Id == id);
            return await GetBankAsync(cursor);
        }

        public async Task UpdateBankAsync(ObjectId id, CreateBankDto dto)
        {
            if (dto.Name == null)
            {
                throw new ValidationException("Bank name is required");
            }

            var cursor = await _collection.FindAsync(a => a.Id == id);
            var bankTask = cursor?.FirstOrDefaultAsync();
            var bank = bankTask != null ? await bankTask : null;

            if (bank == null)
            {
                throw new ValidationException("The bank does not exist");
            }

            bank.Metadata  = dto.Metadata;
            bank.Name      = dto.Name;
            bank.ShortName = dto.ShortName;
            bank.CBNCode   = dto.CBNCode;
            bank.NIPCode   = dto.NIPCode;
        }

        public async Task<ICollection<BankDto>> GetBanksAsync(string query)
        {
            var regex = new BsonRegularExpression($".*{query}.*");
            var cursor = await _collection.FindAsync(Builders<Bank>.Filter.Or(
                Builders<Bank>.Filter.Regex(bank => bank.Name, regex),
                Builders<Bank>.Filter.Eq(bank => bank.ShortName, query)
            ));

            var banks = await cursor.ToListAsync();
            if (banks == null)
            {
                return ArraySegment<BankDto>.Empty;
            }

            return banks.Select(b => new BankDto
            {
                Id        = b.Id,
                Name      = b.Name,
                ShortName = b.ShortName,
                CBNCode   = b.CBNCode,
                NIPCode   = b.NIPCode,
                Metadata  = b.Metadata
            }).ToList();
        }

        public async Task<BankDto?> GetBanksNIPAsync(string nipCode)
        {
            var cursor = await _collection.FindAsync(a => a.NIPCode == nipCode);
            return await GetBankAsync(cursor);
        }

        public async Task<BankDto?> GetBanksCBNAsync(string cbnCode)
        {
            var cursor = await _collection.FindAsync(a => a.NIPCode == cbnCode);
            return await GetBankAsync(cursor);
        }

        private static async Task<BankDto?> GetBankAsync(IAsyncCursor<Bank> cursor)
        {
            var bankTask = cursor?.FirstOrDefaultAsync();
            var bank = bankTask != null ? await bankTask : null;

            if (bank == null)
            {
                return null;
            }

            return new BankDto
            {
                Id        = bank.Id,
                Metadata  = bank.Metadata,
                Name      = bank.Name,
                ShortName = bank.ShortName,
                CBNCode   = bank.CBNCode,
                NIPCode   = bank.NIPCode
            };
        }
    }
}