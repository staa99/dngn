using System.Threading.Tasks;

namespace DngnApiBackend.Integrations.BankUtilities
{
    public interface IBankListService
    {
        Task<BankListServiceResponse> GetBanksAsync();
    }
}