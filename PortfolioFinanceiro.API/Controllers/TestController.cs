using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Data;

namespace PortfolioFinanceiro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DataContext _context;

        public TestController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/assets/marketdata
        /// Retorna todos os MarketData
        /// </summary>
        [HttpGet("marketdata")]
        public async Task<ActionResult<IEnumerable<MarketData>>> GetMarketData()
        {
            var marketData = await _context.MarketData.ToListAsync();

            return Ok(marketData);
        }

        /// <summary>
        /// GET /api/assets/portfolios
        /// Retorna todos os portfolios
        /// </summary>
        [HttpGet("portfolios")]
        public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolio()
        {
            var assets = await _context.Portfolios.ToListAsync();

            return Ok(assets);
        }
    }
}
