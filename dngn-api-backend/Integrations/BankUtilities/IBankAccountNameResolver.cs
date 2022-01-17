using System.Threading.Tasks;

namespace DngnApiBackend.Integrations.BankUtilities
{
    public interface IBankAccountNameResolver
    {
        Task<ResolveAccountNameOutput> ResolveBankAccountNameAsync(string accountNumber, string bankCode);
    }
}