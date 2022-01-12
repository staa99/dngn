using System.Threading.Tasks;
using DngnApiBackend.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DngnApiBackend.Controllers
{
    [Route("banks")]
    public class BankController : BaseController
    {
        private readonly IBankRepository _bankRepository;

        public BankController(IBankRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }


        [HttpGet("")]
        public async Task<IActionResult> GetBanksAsync(string? query)
        {
            var banks = await _bankRepository.GetBanksAsync(query);
            return Ok(new
            {
                status  = "success",
                message = "Banks loaded successfully",
                data    = banks
            });
        }
    }
}