using System.Threading.Tasks;
using DngnApiBackend.Integrations.Blockchain.Incoming;

namespace DngnApiBackend.Services.Platform.Withdrawals
{
    public interface IWithdrawalService
    {
        Task WithdrawAsync(BlockchainIncomingInstruction instruction);
    }
}