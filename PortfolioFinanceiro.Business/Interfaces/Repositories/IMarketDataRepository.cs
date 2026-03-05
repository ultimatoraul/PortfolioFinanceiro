using PortfolioFinanceiro.Business.Models;

namespace PortfolioFinanceiro.Business.Interfaces.Repositories
{
    public interface IMarketDataRepository
    {
        MarketData? GetLastMarketData();
    }
}
