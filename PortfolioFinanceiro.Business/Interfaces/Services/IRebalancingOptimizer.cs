using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Otimização de rebalanceamento
    /// </summary>
    public interface IRebalancingOptimizer
    {
        /// <summary>
        /// Gera sugestões de rebalanceamento para o portfólio baseado em seu ID. <br/>
        /// Este método analisa as posições atuais versus os pesos alvo e recomenda transações para: <br/>
        /// - Corrigir desvios de alocação maiores que 2% <br/>
        /// - Reduzir concentração de risco <br/>
        /// - Otimizar a distribuição de ativos <br/>
        /// - Minimizar custos de transação
        /// </summary>
        /// <param name="id">Identificador único do portfólio a ser rebalanceado.</param>
        /// <returns>
        /// Objeto <see cref="RebalancingSuggestionsResponse"/> contendo: <br/>
        /// - NeedsRebalancing: indica se rebalanceamento é necessário (bool) <br/>
        /// - CurrentAllocation: lista de alocações atuais por ativo <br/>
        /// - SuggestedTrades: lista de transações recomendadas (BUY/SELL) <br/>
        /// - TotalTransactionCost: custo total estimado em transações (0.3% por operação) <br/>
        /// - ExpectedImprovement: descrição da melhoria esperada com o rebalanceamento
        /// </returns>
        /// <remarks>
        /// Regras de Rebalanceamento: <br/>
        /// - Apenas desvios &gt; 2% geram sugestões de trade <br/>
        /// - Trades com valor &lt; R$ 100 são filtrados <br/>
        /// - Prioriza maiores desvios para otimizar eficiência <br/>
        /// - Custo de transação: 0.3% do valor da operação
        /// </remarks>
        RebalancingSuggestionsResponse ByPortfolioId(long id);
    }
}
