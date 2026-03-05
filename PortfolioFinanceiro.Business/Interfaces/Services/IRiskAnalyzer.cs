using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Análise de risco
    /// </summary>
    public interface IRiskAnalyzer
    {
        /// <summary>
        /// Realiza uma análise completa de risco do portfólio baseado em seu ID. <br/>
        /// Este método calcula várias métricas de risco, incluindo: <br/>
        /// - Sharpe Ratio: medida de retorno ajustado ao risco <br/>
        /// - Concentração de Risco: análise das maiores posições e top 3 concentração <br/>
        /// - Diversificação por Setor: alocação percentual e risco por setor <br/>
        /// - Risco Geral: classificação do risco do portfólio (Baixo, Médio, Alto)
        /// </summary>
        /// <param name="id">Identificador único do portfólio a ser analisado.</param>
        /// <returns>
        /// Objeto <see cref="RiskAnalysisResponse"/> contendo: <br/>
        /// - OverallRisk: classificação do risco geral (string) <br/>
        /// - SharpeRatio: índice Sharpe do portfólio (decimal) <br/>
        /// - ConcentrationRisk: dados sobre concentração de risco <br/>
        /// - SectorDiversification: lista de alocações por setor <br/>
        /// - Recommendations: lista de recomendações para mitigação de risco
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando:
        /// - Portfólio não é encontrado (ID inválido)
        /// - Portfólio não possui posições
        /// - Portfólio tem valor total zero
        /// - Dados de mercado (Selic Rate) não estão disponíveis
        /// </exception>
        RiskAnalysisResponse ByPortfolioId(long id);
    }
}
