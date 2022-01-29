using System.Threading.Tasks;

namespace DngnApiBackend.Data.Seeds
{
    public interface IBankDataSeeder
    {
        Task SeedBanksDataAsync();
    }
}