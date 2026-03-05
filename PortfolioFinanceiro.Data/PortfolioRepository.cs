using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;

namespace PortfolioFinanceiro.Data
{
    public class PortfolioRepository(DataContext context) : IPortfolioRepository
    {
        private readonly DataContext _context = context;

        public Portfolio? GetPortfolioWithPositions(long id)
        {
            return _context.Portfolios
                .Include(p => p.Positions)
                .FirstOrDefault(p => p.Id == id);
        }

        public Asset? GetAssetWithPriceHistory(string symbol)
        {
            return _context.Assets
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
        }

        public List<PriceHistory> GetPriceHistoryBySymbol(string symbol)
        {
            return _context.PriceHistory
                .Where(ph => ph.Symbol == symbol)
                .OrderBy(ph => ph.Date)
                .ToList();
        }
    }
}
