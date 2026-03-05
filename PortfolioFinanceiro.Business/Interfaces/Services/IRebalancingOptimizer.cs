using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Otimização de rebalanceamento
    /// </summary>
    public interface IRebalancingOptimizer
    {
        RebalancingSuggestions ByPortfolioId(long id);
    }
}
