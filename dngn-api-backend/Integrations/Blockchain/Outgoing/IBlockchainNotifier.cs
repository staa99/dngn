using System.Threading.Tasks;

namespace DngnApiBackend.Integrations.Blockchain.Outgoing
{
    public interface IBlockchainNotifier
    {
        Task TriggerMinter(BlockchainOutgoingInstruction instruction);
    }
}