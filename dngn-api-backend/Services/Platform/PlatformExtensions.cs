using DngnApiBackend.Configuration;
using DngnApiBackend.Services.Platform.Pricing;
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
            
            services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();
            return services;
        }
    }
}