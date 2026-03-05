using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.Business.Interfaces.Services
{
    /// <summary>
    /// Algoritmos de performance
    /// </summary>
    public interface IPerformanceCalculator
    {
        /// - Sharpe Ratio: medida de retorno ajustado ao risco <br/>
        /// - Concentração de Risco: análise das maiores posições e top 3 concentração <br/>
        /// - Diversificação por Setor: alocação percentual e risco por setor <br/>
        /// - Risco Geral: classificação do risco do portfólio (Baixo, Médio, Alto)
        /// 
        /// 
        /// <summary>
        /// Analisa a performance de um portfólio e retorna métricas detalhadas. <br/> <br/>
        /// Este método realiza uma análise completa do portfólio, calculando: <br/>
        /// - Valor Investido Total: Soma de (quantidade * preço médio) de todas as posições. <br/>
        /// - Valor Atual do Portfólio: Soma de (quantidade * preço atual) de todas as posições. <br/>
        /// - Retorno por Posição: Percentual de ganho/perda por ativo, calculado como:
        ///    ((Valor Atual - Valor Investido) / Valor Investido) * 100 <br/>
        /// - Peso de Cada Posição: Percentual que cada posição representa no portfólio total:
        ///    (Valor Atual da Posição / Valor Total Atual do Portfólio) * 100 <br/>
        /// - Retorno Total do Portfólio: Percentual de ganho/perda geral:
        ///    ((Valor Atual Total - Investimento Total) / Investimento Total) * 100 <br/>
        /// - Valor Absoluto do Retorno (TotalReturnAmount)**: 
        ///    Diferença em reais entre o valor atual e o investido. <br/>
        /// - Retorno Anualizado: Projeção do retorno para um período de 12 meses,
        ///    calculado usando a fórmula: ((1 + Retorno Total)^(365/dias) - 1) * 100
        ///    Útil para comparar performance de portfólios com diferentes períodos de investimento. <br/>
        /// - Volatilidade: Desvio padrão dos retornos diários percentuais,
        ///    medindo a flutuação do preço e o risco associado.
        ///    Retorna null se não houver histórico de preços suficiente. <br/>
        /// Todos os valores decimais são automaticamente limitados a 2 casas decimais
        /// para garantir precisão financeira adequada. <br/>
        /// Posições sem ativo correspondente são ignoradas silenciosamente durante o cálculo.
        /// </summary>
        /// <param name="id">ID do portfólio a ser analisado.</param>
        /// <returns>
        /// Objeto PerfomanceResult contendo todas as métricas de performance calculadas. <br/>
        /// Retorna um resultado com valores zerados se não houver posições válidas.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o portfólio com o ID fornecido não é encontrado.
        /// </exception>
        PerfomanceResponse ByPortfolioId(long id);
    }
}
