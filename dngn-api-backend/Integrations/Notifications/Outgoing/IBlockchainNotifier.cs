using System.Threading.Tasks;

namespace DngnApiBackend.Integrations.Notifications.Outgoing
{
    public interface IBlockchainNotifier
    {
        Task TriggerMinter(BlockchainTransactionInstruction instruction);
    }
}