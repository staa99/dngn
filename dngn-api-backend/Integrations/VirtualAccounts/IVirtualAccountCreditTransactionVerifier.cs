using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.LogVirtualAccountTransaction;

namespace DngnApiBackend.Integrations.VirtualAccounts
{
    public interface IVirtualAccountCreditTransactionVerifier
    {
        Task<LogVirtualAccountTransactionInput?> VerifyVirtualAccountCreditTransactionAsync(string providerReference);
    }
}