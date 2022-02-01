using System.Threading.Tasks;
using DngnApiBackend.Data.Models;

namespace DngnApiBackend.Integrations.BankUtilities
{
    public interface IBankAccountNameResolver
    {
        BankMetaKey BankCodeMetaKey { get; }
        Task<ResolveAccountNameOutput> ResolveBankAccountNameAsync(string accountNumber, string bankCode);
    }
}