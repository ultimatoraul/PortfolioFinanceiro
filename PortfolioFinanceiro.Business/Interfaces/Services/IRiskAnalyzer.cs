using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Análise de risco
    /// </summary>
    public interface IRiskAnalyzer
    {
        RiskAnalysisResult ByPortfolioId(long id);
    }
}
