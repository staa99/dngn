using DngnApiBackend.Configuration;
using DngnApiBackend.Data.Seeds;
using DngnApiBackend.Integrations.VirtualAccounts;
using DngnApiBackend.Services.Platform.Pricing;
using DngnApiBackend.Services.Platform.Withdrawals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DngnApiBackend.Services.Platform
{
    public static class PlatformExtensions
    {
        public static IServiceCollection AddPlatformServices(this IServiceCollection services)
        {
            services.AddOptions<DngnOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("DNGN").Bind(settings);
                });

            services.AddScoped<IPriceCalculatorService, PriceCalculatorService>()
                .AddScoped<IVirtualAccountCreator, VirtualAccountCreator>()
                .AddScoped<IWithdrawalService, WithdrawalService>()
                .AddScoped<IBankDataSeeder, BankDataSeeder>();
            return services;
        }
    }
}