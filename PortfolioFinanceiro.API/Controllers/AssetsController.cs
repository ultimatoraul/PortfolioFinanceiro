using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Data;

namespace PortfolioFinanceiro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly DataContext _context;

        public AssetsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/assets
        /// Retorna todos os ativos carregados do SeedData.json
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
        {
            var assets = await _context.Assets
                .ToListAsync();
            
            return Ok(assets);
        }

        /// <summary>
        /// GET /api/assets/{symbol}
        /// Retorna um ativo específico com histórico de preços
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<Asset>> GetAsset(string symbol)
        {
            var asset = _context.Assets
                  .Where(a => a.Symbol == symbol)
                  .Select(a => new Asset
                  {
                      Symbol = a.Symbol,
                      Name = a.Name,
                      Type = a.Type,
                      Sector = a.Sector,
                      CurrentPrice = a.CurrentPrice,
                      LastUpdated = a.LastUpdated,
                      PriceHistory = _context.PriceHistory
                          .Where(ph => ph.Symbol == symbol)
                          .ToList()
                  })
                  .FirstOrDefault();

            if (asset == null)
                return NotFound($"Ativo {symbol} não encontrado");

            return Ok(asset);
        }

        /// <summary>
        /// GET /api/assets/sector/{sector}
        /// Retorna todos os ativos de um setor específico
        /// </summary>
        [HttpGet("sector/{sector}")]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssetsBySector(string sector)
        {
            var assets = await _context.Assets
                .Where(a => a.Sector.ToLower() == sector.ToLower()).ToListAsync();

            if (!assets.Any())
                return NotFound($"Nenhum ativo encontrado no setor {sector}");

            return Ok(assets);
        }

        /// <summary>
        /// GET /api/assets/{symbol}/pricehistory
        /// Retorna o histórico de preços de um ativo
        /// </summary>
        [HttpGet("{symbol}/pricehistory")]
        public async Task<ActionResult<IEnumerable<PriceHistory>>> GetPriceHistory(string symbol)
        {
            var priceHistory = await _context.PriceHistory
                .Where(a => a.Symbol == symbol).ToListAsync();

            if (!priceHistory.Any())
                return NotFound($"Ativo {symbol} não encontrado");
            
            return Ok(priceHistory.OrderByDescending(ph => ph.Date));
        }
    }
}
