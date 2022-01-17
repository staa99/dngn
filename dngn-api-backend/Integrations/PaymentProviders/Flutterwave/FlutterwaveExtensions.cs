using System;
using System.Net.Http.Headers;
using DngnApiBackend.Integrations.BankUtilities;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.WebHooks;
using DngnApiBackend.Integrations.Transfers;
using DngnApiBackend.Integrations.VirtualAccounts;
using DngnApiBackend.Services.WebhookServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave
{
    public static class FlutterwaveExtensions
    {
        public static IServiceCollection AddFlutterwave(this IServiceCollection services)
        {
            services.AddOptions<FlutterwaveOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Flutterwave").Bind(settings);
                });

            services.AddHttpClient<Flutterwave>((serviceProvider, flutterwave) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FlutterwaveOptions>>();
                flutterwave.BaseAddress = new Uri(options.Value.BaseUri);
                flutterwave.DefaultRequestHeaders.Authorization =
                    AuthenticationHeaderValue.Parse($"Bearer {options.Value.SecretKey}");
            });

            services.AddSingleton<IProviderVirtualAccountCreator>(serviceProvider =>
                serviceProvider.GetRequiredService<Flutterwave>());
            services.AddSingleton<IVirtualAccountCreditTransactionVerifier>(serviceProvider =>
                serviceProvider.GetRequiredService<Flutterwave>());
            services.AddSingleton<IBankAccountNameResolver>(serviceProvider =>
                serviceProvider.GetRequiredService<Flutterwave>());
            services.AddSingleton<IBankListService>(
                serviceProvider => serviceProvider.GetRequiredService<Flutterwave>());
            services.AddSingleton<ITransferProcessor>(serviceProvider =>
                serviceProvider.GetRequiredService<Flutterwave>());
            services.AddSingleton<ITransferQueryProcessor>(serviceProvider =>
                serviceProvider.GetRequiredService<Flutterwave>());
            services.AddScoped<IInwardBankTransferHook, FlutterwaveInwardBankTransferHook>();
            services.AddScoped<IOutwardBankTransferHook, FlutterwaveOutwardBankTransferHook>();

            return services;
        }
    }
}