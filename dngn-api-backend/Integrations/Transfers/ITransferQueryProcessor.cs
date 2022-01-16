using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.QueryTransfer;

namespace DngnApiBackend.Integrations.Transfers
{
    public interface ITransferQueryProcessor
    {
        Task<QueryTransferOutput?> QueryTransferAsync(string providerReference);
    }
}