using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.InitiateTransfer;

namespace DngnApiBackend.Integrations.Transfers
{
    public interface ITransferProcessor
    {
        Task<InitiateTransferOutput> InitiateTransferAsync(InitiateTransferInput input);
    }
}