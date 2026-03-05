using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Algoritmos de performance
    /// </summary>
    public interface IPerformanceCalculator
    {
        Perfomance ToAnalyze(long id);
    }
}
