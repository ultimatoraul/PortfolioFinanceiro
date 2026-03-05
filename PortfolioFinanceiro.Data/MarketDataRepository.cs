using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;

namespace PortfolioFinanceiro.Data
{
    public class MarketDataRepository(DataContext context) : IMarketDataRepository
    {
        private readonly DataContext _context = context;

        public MarketData? GetLastMarketData()
        {
            return _context.MarketData
                .Include(p => p.IndexPerformance)
                .Include(p => p.Sectors)
                .LastOrDefault();
        }
    }
}
