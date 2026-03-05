using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Interfaces.Services;

namespace PortfolioFinanceiro.Business.Services
{
    public class PerformanceCalculator(IPortfolioRepository repository) : IPerformanceCalculator
    {
        private readonly IPortfolioRepository _repository = repository;

        public PerfomanceResult ToAnalyze(long id)
        {
            var portfolioObj = _repository.GetPortfolioWithPositions(id);

            if (portfolioObj == null)
                throw new InvalidOperationException(PortfolioBusinessResource.PortfolioIdInvalid + $" ({id})");

            var performanceResult = new PerfomanceResult();

            // Calcular o valor atual de cada posição (positionsPerformance)
            foreach (var position in portfolioObj.Positions!)
            {
                var asset = _repository.GetAssetWithPriceHistory(position.Symbol);

                if (asset == null)
                    continue;

                decimal investedAmount = position.Quantity * position.AveragePrice;
                decimal currentPositionValue = position.Quantity * asset.CurrentPrice;

                // Calcular a Variação Percentual do Ativo (ROI) por meio de regra de três composta:
                // (ValorDaPosicaoAtual - ValorInvestido) * 100 / ValorInvestido
                decimal positionReturn = investedAmount > 0
                    ? (currentPositionValue - investedAmount) * 100 / investedAmount
                    : 0;

                // Calcular o valor total investido e do portfolio atual
                performanceResult.TotalInvestment += investedAmount;
                performanceResult.CurrentValue += currentPositionValue;

                performanceResult.PositionsPerformance.Add(new PositionPerformance
                {
                    Symbol = position.Symbol,
                    InvestedAmount = investedAmount,
                    CurrentValue = currentPositionValue,
                    Return = positionReturn
                });
            }

            // Calcular o peso de cada posição no portfólio (Weight) usando a regra de três simples, usando como base o valor do portfolio atual obtido:
            // (ValorDaPosicaoAtual / ValorDoPortfolioAtual) * 100
            foreach (var positionPerf in performanceResult.PositionsPerformance!)
            {
                positionPerf.Weight = performanceResult.CurrentValue > 0
                        ? (positionPerf.CurrentValue / performanceResult.CurrentValue) * 100
                        : 0;
            }

            // Calcular Total Return: (ValorAtual - ValorInvestido) / ValorInvestido * 100
            if (performanceResult.TotalInvestment > 0)
            {
                performanceResult.TotalReturnAmount = performanceResult.CurrentValue - performanceResult.TotalInvestment;
                performanceResult.TotalReturn = (performanceResult.TotalReturnAmount / performanceResult.TotalInvestment) * 100;
            }

            // Calcular Annualized Return: ((1 + TotalReturn)^(365/dias) - 1) * 100
            var daysHeld = (decimal)(DateTime.UtcNow - portfolioObj.CreatedAt).TotalDays;
            if (daysHeld > 0 && performanceResult.TotalReturn > -100)
            {
                decimal totalReturnDecimal = performanceResult.TotalReturn / 100;
                performanceResult.AnnualizedReturn = ((decimal)Math.Pow((double)(1 + totalReturnDecimal), 365 / (double)daysHeld) - 1) * 100;
            }

            // Calcular Volatility: Desvio padrão dos retornos diários usando PriceHistory
            performanceResult.Volatility = CalculateVolatility(portfolioObj.Positions);
            return performanceResult;
        }

        private decimal? CalculateVolatility(List<Models.Position> positions)
        {
            var dailyReturns = new List<decimal>();

            // 1. Calcular o percentual do Retorno Diário ((precoDeHoje - precoDeOntem) / precoDeOntem) * 100
            foreach (var position in positions)
            {
                var priceHistoryObj = _repository.GetPriceHistoryBySymbol(position.Symbol);

                if (priceHistoryObj == null || priceHistoryObj.Count < 2)
                    continue;

                for (int i = 1; i < priceHistoryObj.Count; i++)
                {
                    var previousPrice = priceHistoryObj[i - 1].Price;
                    if (previousPrice > 0)
                    {
                        var dailyReturn = ((priceHistoryObj[i].Price - previousPrice) / previousPrice) * 100;
                        dailyReturns.Add(dailyReturn);
                    }
                }
            }

            // 1.1 Edge Case para volatility, sem histórico de preços
            if (dailyReturns.Count == 0)
                return null;

            // 2. Média dos Retornos
            decimal averageReturn = dailyReturns.Average();

            // 3. Soma dos Quadrados das Diferenças (Variância)
            double variance = dailyReturns.Sum(r => Math.Pow((double)(r - averageReturn), 2) / dailyReturns.Count);

            // 4. Desvio Padrão (Volatilidade Diária)
            decimal standardDeviation = (decimal)Math.Sqrt(variance);

            return standardDeviation;
        }
    }
}
