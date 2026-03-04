using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces
{
    /// <summary>
    /// Algoritmos de performance
    /// </summary>
    public interface IPerformanceCalculator
    {
        Perfomance Performance(string id);
    }
}
