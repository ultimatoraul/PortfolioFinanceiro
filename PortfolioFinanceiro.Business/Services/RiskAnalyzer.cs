using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Services;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Business.Utils;

namespace PortfolioFinanceiro.Business.Services
{
    public class RiskAnalyzer(IPortfolioRepository portfolioRepository, IMarketDataRepository marketDataRepository) : IRiskAnalyzer
    {
        private readonly IPortfolioRepository _portfolioRepository = portfolioRepository;
        private readonly IMarketDataRepository _marketDataRepository = marketDataRepository;

        public RiskAnalysisResponse ByPortfolioId(long id)
        {
            var portfolio = _portfolioRepository.GetPortfolioWithPositions(id);

            if (portfolio == null)
                throw new InvalidOperationException(PortfolioBusinessResource.PortfolioIdInvalid + $" ({id})");

            var positions = portfolio.Positions;
            if (positions == null || positions.Count == 0)
                throw new InvalidOperationException(PortfolioBusinessResource.PortfolioHasNoPositions);

            // Calcular métricas do portfolio
            Dictionary<string, decimal> positionValues = CalculatePositionValues(positions);
            var totalValue = positionValues.Values.Sum();

            if (totalValue == 0)
                throw new InvalidOperationException(PortfolioBusinessResource.PortfolioHasNoValue);

            // Calcular percentual das positions
            Dictionary<string, decimal> positionPercentages = positionValues.ToDictionary(
                kv => kv.Key,
                kv => kv.Value / totalValue
            );

            // Obter sector allocations
            Dictionary<string, decimal> sectorAllocation = CalculateSectorAllocation(positions, positionPercentages);

            // Calcular Sharpe Ratio
            decimal portfolioReturn = CalculatePortfolioReturn(positions);
            decimal volatility = CalculateVolatility(positions);
            decimal selicRate = GetSelicRate();
            decimal sharpeRatio = CalculateSharpeRatio(portfolioReturn, volatility, selicRate);

            // Analisar concentração de risco
            var largestPosition = positionPercentages.OrderByDescending(p => p.Value).FirstOrDefault();
            var top3Concentration = positionPercentages
                .OrderByDescending(p => p.Value)
                .Take(3)
                .Sum(p => p.Value);

            var concentrationRisk = new ConcentrationRisk
            {
                LargestPosition = new LargestPosition
                {
                    Symbol = largestPosition.Key,
                    Percentage = largestPosition.Value * 100
                },
                Top3Concentration = top3Concentration * 100
            };

            // Determinar o risco geral
            var overallRisk = RiskFunctions.DetermineOverallRisk(positionPercentages, sectorAllocation);

            // Converter o sector allocation para o formato DTO
            var sectorDiversificationList = ConvertSectorAllocationToDTO(sectorAllocation);

            // Gerar recomendações
            var recommendations = RiskFunctions.GenerateRecommendations(positionPercentages, sectorAllocation);

            return new RiskAnalysisResponse
            {
                OverallRisk = overallRisk,
                SharpeRatio = sharpeRatio,
                ConcentrationRisk = concentrationRisk,
                SectorDiversification = sectorDiversificationList,
                Recommendations = recommendations
            };
        }

        private static decimal CalculateSharpeRatio(decimal portfolioReturn, decimal volatility, decimal riskFreeRate)
        {
            if (volatility == 0)
                return 0;

            return (portfolioReturn - riskFreeRate) / volatility;
        }

        private Dictionary<string, decimal> CalculatePositionValues(List<Position> positions)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var position in positions)
            {
                var asset = _portfolioRepository.GetAssetWithPriceHistory(position.Symbol);
                if (asset != null)
                {
                    var value = position.Quantity * asset.CurrentPrice;
                    result[position.Symbol] = value;
                }
            }

            return result;
        }

        private Dictionary<string, decimal> CalculateSectorAllocation(
            List<Position> positions,
            Dictionary<string, decimal> positionPercentages)
        {
            var sectorAllocation = new Dictionary<string, decimal>();

            foreach (var position in positions)
            {
                var asset = _portfolioRepository.GetAssetWithPriceHistory(position.Symbol);
                if (asset != null && positionPercentages.TryGetValue(position.Symbol, out var percentage))
                {
                    var sector = asset.Sector ?? "Unknown";

                    if (!sectorAllocation.ContainsKey(sector))
                        sectorAllocation[sector] = 0;

                    sectorAllocation[sector] += percentage;
                }
            }

            return sectorAllocation;
        }

        private decimal CalculatePortfolioReturn(List<Position> positions)
        {
            // Cálculo simplificado do retorno com base no histórico de preços
            decimal totalReturn = 0;
            decimal totalWeight = 0;

            foreach (var position in positions)
            {
                var asset = _portfolioRepository.GetAssetWithPriceHistory(position.Symbol);
                if (asset != null && asset.PriceHistory.Count > 1)
                {
                    var currentPrice = asset.CurrentPrice;
                    var previousPrice = asset.PriceHistory.OrderByDescending(p => p.Date).Skip(1).FirstOrDefault()?.Price ?? currentPrice;

                    if (previousPrice > 0)
                    {
                        var returnRate = (currentPrice - previousPrice) / previousPrice;
                        var weight = position.Quantity * currentPrice;
                        totalReturn += weight * returnRate;
                        totalWeight += weight;
                    }
                }
            }

            return totalWeight > 0 ? totalReturn / totalWeight : 0;
        }

        private decimal CalculateVolatility(List<Position> positions)
        {
            // Cálculo simplificado da volatilidade usando a variação do histórico de preços
            var returns = new List<decimal>();

            foreach (var position in positions)
            {
                var asset = _portfolioRepository.GetAssetWithPriceHistory(position.Symbol);
                if (asset != null && asset.PriceHistory.Count > 1)
                {
                    var sortedHistory = asset.PriceHistory.OrderBy(p => p.Date).ToList();

                    for (int i = 1; i < sortedHistory.Count; i++)
                    {
                        if (sortedHistory[i - 1].Price > 0)
                        {
                            var returnRate = (sortedHistory[i].Price - sortedHistory[i - 1].Price) / sortedHistory[i - 1].Price;
                            returns.Add(returnRate);
                        }
                    }
                }
            }

            if (returns.Count < 2)
                return 0;

            var mean = returns.Average();
            var variance = returns.Sum(r => (r - mean) * (r - mean)) / returns.Count;

            return (decimal)Math.Sqrt((double)variance);
        }

        private decimal GetSelicRate()
        {
            var marketData = _marketDataRepository.GetLastMarketData();
            if (marketData != null)
                return marketData.SelicRate;
            else
                throw new InvalidOperationException(PortfolioBusinessResource.MarketDataNotFound);
        }

        private static List<SectorDiversification> ConvertSectorAllocationToDTO(Dictionary<string, decimal> sectorAllocation)
        {
            return sectorAllocation.Select(s => new SectorDiversification
            {
                Sector = s.Key,
                Percentage = s.Value * 100,
                Risk = RiskFunctions.DetermineSectorRisk(s.Value)
            }).OrderByDescending(s => s.Percentage).ToList();
        }
    }
}
