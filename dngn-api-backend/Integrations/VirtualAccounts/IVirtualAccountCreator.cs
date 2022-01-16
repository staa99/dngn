using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;

namespace DngnApiBackend.Integrations.VirtualAccounts
{
    public interface IVirtualAccountCreator
    {
        Task<CreateVirtualAccountOutput> CreateVirtualAccountAsync(string ownerId, CreateVirtualAccountInput input);
    }
}