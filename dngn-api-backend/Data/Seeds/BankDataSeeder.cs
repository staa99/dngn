using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Integrations.BankUtilities;
using DngnApiBackend.Services.Repositories;
using Microsoft.Extensions.Configuration;

namespace DngnApiBackend.Data.Seeds
{
    public class BankDataSeeder : IBankDataSeeder
    {
        private readonly IBankListService _bankListService;
        private readonly IBankRepository _bankRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _loader;

        public BankDataSeeder(IConfiguration configuration, IBankListService bankListService,
            IBankRepository bankRepository)
        {
            _configuration   = configuration;
            _bankListService = bankListService;
            _bankRepository  = bankRepository;
            _loader          = new HttpClient();
        }


        public async Task SeedBanksDataAsync()
        {
            var existingBanks = await _bankRepository.GetBanksAsync("");
            if (existingBanks.Any())
            {
                return;
            }

            var data = await _loader.GetStringAsync(_configuration["DataSeed:BankSeedURL"]);
            var seedData = JsonSerializer.Deserialize<BankSeedModel>(data);
            if (seedData == null)
            {
                throw new ApplicationException("BANK_DATA_SEED_FAILED");
            }

            var providerData = await _bankListService.GetBanksAsync();
            var indexedSeedData = new Dictionary<string, BankSeedItemModel>();
            foreach (var bank in seedData.Banks)
            {
                indexedSeedData[bank.CBNCode] = bank;
                indexedSeedData[bank.NIPCode] = bank;
            }

            var bankEntities = (from bank in providerData.Data
                let matchingSeed = indexedSeedData.GetValueOrDefault(bank.Codes[BankCodeType.FlutterwaveCode])
                select new Bank
                {
                    Name        = matchingSeed?.Name ?? bank.Name,
                    NormalizedName        = (matchingSeed?.Name ?? bank.Name).ToLowerInvariant(),
                    CBNCode     = matchingSeed?.CBNCode,
                    NIPCode     = matchingSeed?.NIPCode,
                    DateCreated = DateTimeOffset.UtcNow,
                    Metadata    = {[BankMetaKey.FlutterwaveCode] = bank.Codes[BankCodeType.FlutterwaveCode]}
                }).ToList();
            await _bankRepository.CreateBanksAsync(bankEntities);
        }
    }
}