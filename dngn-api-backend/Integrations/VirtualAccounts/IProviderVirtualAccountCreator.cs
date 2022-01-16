using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;

namespace DngnApiBackend.Integrations.VirtualAccounts
{
    public interface IProviderVirtualAccountCreator
    {
        /// <summary>
        ///     Creates the VirtualAccount on the provider's side
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<CreateVirtualAccountOutput> CreateVirtualAccountAsync(CreateVirtualAccountInput input);
    }
}