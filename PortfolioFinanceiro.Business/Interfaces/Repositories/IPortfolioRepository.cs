using PortfolioFinanceiro.Business.Models;

namespace PortfolioFinanceiro.Business.Interfaces.Repositories
{
    public interface IPortfolioRepository
    {
        Portfolio? GetPortfolioWithPositions(long id);
        Asset? GetAssetWithPriceHistory(string symbol); 
        List<PriceHistory> GetPriceHistoryBySymbol(string symbol);
    }
}
