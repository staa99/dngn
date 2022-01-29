using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Data.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace DngnApiBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            var builder = CreateHostBuilder(args).Build();
            await SetupDbAsync(builder);
            await builder.RunAsync();
        }

        private static async Task SetupDbAsync(IHost builder)
        {
            using var scope = builder.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            await db.RegisterDngnIndexes();

            var bankDataSeeder = scope.ServiceProvider.GetRequiredService<IBankDataSeeder>();
            await bankDataSeeder.SeedBanksDataAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}