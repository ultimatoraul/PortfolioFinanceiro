using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Interfaces.Services;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Business.Utils;

namespace PortfolioFinanceiro.Business.Services
{
    public class RebalancingOptimizer(IPortfolioRepository portfolioRepository) : IRebalancingOptimizer
    {
        private readonly IPortfolioRepository _portfolioRepository = portfolioRepository;
        private const decimal MinimumDeviation = 2m; // Desvio mínimo de 2%
        private const decimal TransactionCostRate = 0.003m; // 0.3% por operação
        private const decimal MinimumTradeValue = 100m; // Mínimo de R$ 100

        public RebalancingSuggestionsResponse ByPortfolioId(long id)
        {
            var portfolio = _portfolioRepository.GetPortfolioWithPositions(id);

            if (portfolio == null || portfolio.Positions.Count == 0)
                return EmptyResponse(PortfolioBusinessResource.PortfolioEmptyOrNotFound);

            var snapshots = LoadSnapshots(portfolio.Positions);
            decimal totalValue = snapshots.Sum(s => s.CurrentValue);

            if (totalValue == 0)
                return EmptyResponse(PortfolioBusinessResource.PortfolioWithoutValue);

            var analyses = BuildAnalyses(snapshots, totalValue);
            var suggestedTrades = BuildSuggestedTrades(analyses, totalValue);

            string expectedImprovement = suggestedTrades.Count > 0
                ? $"Redução de {decimal.Round(analyses.Where(a => a.Deviation > MinimumDeviation).Average(a => a.Deviation), 1)}% no desvio médio de alocação"
                : "Portfólio já está bem alocado";

            return new RebalancingSuggestionsResponse
            {
                NeedsRebalancing = suggestedTrades.Count > 0,
                CurrentAllocation = analyses.Select(a => new Allocation
                {
                    Symbol = a.Position.Symbol,
                    CurrentWeight = a.CurrentWeight,
                    TargetWeight = a.TargetWeight,
                    Deviation = a.Deviation
                }).ToList(),
                SuggestedTrades = suggestedTrades,
                TotalTransactionCost = suggestedTrades.Sum(t => t.TransactionCost),
                ExpectedImprovement = expectedImprovement
            };
        }

        private List<PositionSnapshot> LoadSnapshots(List<Position> positions)
        {
            var snapshots = new List<PositionSnapshot>();
            foreach (var position in positions)
            {
                var asset = _portfolioRepository.GetAssetWithPriceHistory(position.Symbol);
                if (asset != null)
                {
                    // Valor atual da posição = quantidade de cotas × preço corrente do ativo
                    decimal currentValue = position.Quantity * asset.CurrentPrice;
                    snapshots.Add(new PositionSnapshot(position, asset, currentValue));
                }
            }
            return snapshots;
        }

        private static List<PositionAnalysis> BuildAnalyses(List<PositionSnapshot> snapshots, decimal totalValue)
        {
            return snapshots.Select(s =>
            {
                // PesoAtual = valorDaPosicao / valorTotalDoPortfolio * 100
                decimal currentWeight = FinancialCalculator.WeightPercentual(s.CurrentValue, totalValue);
                decimal targetWeight = s.Position.TargetAllocation;

                // Desvio absoluto entre o peso atual e o peso-alvo definido pelo investidor
                decimal deviation = Math.Abs(currentWeight - targetWeight);

                return new PositionAnalysis(s.Position, s.Asset, s.CurrentValue, currentWeight, targetWeight, deviation);
            }).ToList();
        }

        private List<SuggestedTrade> BuildSuggestedTrades(List<PositionAnalysis> analyses, decimal portfolioTotalValue)
        {
            var trades = new List<SuggestedTrade>();

            // Valor temporário do portfólio
            var tempValue = portfolioTotalValue;

            // Processa apenas posições com desvio acima do mínimo
            foreach (var a in analyses.Where(a => a.Deviation > MinimumDeviation).OrderByDescending(a => a.Deviation))
            {
                // Valor-alvo que a posição deveria ter dado o peso-alvo e o valor atual do portfólio
                decimal targetValue = (a.TargetWeight / 100m) * tempValue;

                // Diferença positiva: posição está acima do alvo (SELL); negativa → abaixo (BUY)
                decimal valueDifference = a.CurrentValue - targetValue;
                decimal amountToTrade = Math.Abs(valueDifference);

                // Ignora operações abaixo do valor mínimo 
                if (amountToTrade < MinimumTradeValue)
                    continue;

                int quantity = (int)Math.Round(amountToTrade / a.Asset.CurrentPrice);
                if (quantity == 0)
                    continue;

                string action = valueDifference > 0 ? "SELL" : "BUY";

                // Valor real da operação
                decimal estimatedValue = quantity * a.Asset.CurrentPrice;

                // Custo de transação calculado
                decimal transactionCost = amountToTrade * TransactionCostRate;

                trades.Add(new SuggestedTrade
                {
                    Symbol = a.Position.Symbol,
                    Action = action,
                    Quantity = quantity,
                    EstimatedValue = estimatedValue,
                    TransactionCost = transactionCost,
                    Reason = action == "SELL"
                        ? $"Reduzir de {decimal.Round(a.CurrentWeight, 1)}% para {decimal.Round(a.TargetWeight, 1)}%"
                        : $"Aumentar de {decimal.Round(a.CurrentWeight, 1)}% para {decimal.Round(a.TargetWeight, 1)}%"
                });

                // Atualiza o valor temporário do portfólio 
                tempValue += action == "SELL" ? -estimatedValue : estimatedValue;
            }

            return trades;
        }

        private static RebalancingSuggestionsResponse EmptyResponse(string message) => new()
        {
            NeedsRebalancing = false,
            CurrentAllocation = [],
            SuggestedTrades = [],
            TotalTransactionCost = 0,
            ExpectedImprovement = message
        };

        private record PositionSnapshot(Position Position, Asset Asset, decimal CurrentValue);

        private record PositionAnalysis(
            Position Position,
            Asset Asset,
            decimal CurrentValue,
            decimal CurrentWeight,
            decimal TargetWeight,
            decimal Deviation);
    }
}
