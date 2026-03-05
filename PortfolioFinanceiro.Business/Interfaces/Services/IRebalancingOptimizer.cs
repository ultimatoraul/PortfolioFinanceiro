using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Análise de risco
    /// </summary>
    public interface IRebalancingOptimizer
    {
        RebalancingSuggestions Rebalancing(string id);
    }
}
