using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Algoritmos de performance
    /// </summary>
    public interface IPerformanceCalculator
    {
        PerfomanceResult ToAnalyze(long id);
    }
}
