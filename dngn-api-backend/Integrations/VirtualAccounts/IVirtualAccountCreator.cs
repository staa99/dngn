using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;
using MongoDB.Bson;

namespace DngnApiBackend.Integrations.VirtualAccounts
{
    public interface IVirtualAccountCreator
    {
        Task<CreateVirtualAccountOutput> CreateVirtualAccountAsync(ObjectId ownerId, CreateVirtualAccountInput input);
    }
}