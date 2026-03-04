using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces
{
    public interface IPortfolioService
    {
        Perfomance Performance(string id);
        RiskAnalysis RiskAnalysis(string id);
        RebalancingSuggestions Rebalancing(string id);
    }
}
