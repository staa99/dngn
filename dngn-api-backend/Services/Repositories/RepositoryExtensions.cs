using Microsoft.Extensions.DependencyInjection;

namespace DngnApiBackend.Services.Repositories
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddDngnRepositories(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IBankRepository, BankRepository>()
                .AddScoped<IBankAccountRepository, BankAccountRepository>()
                .AddScoped<IUserAccountRepository, UserAccountRepository>()
                .AddScoped<IDepositRepository, DepositRepository>()
                .AddScoped<IWithdrawalRepository, WithdrawalRepository>();
        }
    }
}