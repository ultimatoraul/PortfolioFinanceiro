namespace PortfolioFinanceiro.Business.Utils
{
    internal class RiskFunctions
    {
        private const decimal SafePositionLimit = 0.20m; // 20%
        private const decimal MediumPositionLower = 0.15m; // 15%
        private const decimal HighPositionLimit = 0.25m; // 25%

        private const decimal SafeSectorLimit = 0.25m; // 25%
        private const decimal MediumSectorLower = 0.25m; // 25%
        private const decimal HighSectorLimit = 0.40m; // 40%


        internal static string DetermineSectorRisk(decimal sectorPercentage)
        {
            if (sectorPercentage > HighSectorLimit)
                return "High";

            if (sectorPercentage >= MediumSectorLower)
                return "Medium";

            return "Low";
        }

        internal static string DetermineOverallRisk(
        Dictionary<string, decimal> positionPercentages,
        Dictionary<string, decimal> sectorAllocation)
        {
            var hasHighRisk = positionPercentages.Any(p => p.Value > HighPositionLimit) ||
                              sectorAllocation.Any(s => s.Value > HighSectorLimit);

            if (hasHighRisk)
                return "High";

            var hasMediumRisk = positionPercentages.Any(p => p.Value >= MediumPositionLower && p.Value <= HighPositionLimit) ||
                                sectorAllocation.Any(s => s.Value >= MediumSectorLower && s.Value <= HighSectorLimit);

            if (hasMediumRisk)
                return "Medium";

            return "Low";
        }

        internal static List<string> GenerateRecommendations(
            Dictionary<string, decimal> positionPercentages,
            Dictionary<string, decimal> sectorAllocation)
        {
            var recommendations = new List<string>();

            // Checar position concentration
            foreach (var position in positionPercentages.Where(p => p.Value > SafePositionLimit).OrderByDescending(p => p.Value))
            {
                var percentage = Math.Round(position.Value * 100, 1);
                recommendations.Add(
                    $"Reduzir exposição à posição {position.Key} ({percentage}% do portfólio, ideal < 20%)"
                );
            }

            // Checar sector concentration
            foreach (var sector in sectorAllocation.Where(s => s.Value > SafeSectorLimit).OrderByDescending(s => s.Value))
            {
                var percentage = Math.Round(sector.Value * 100, 1);
                recommendations.Add(
                    $"Reduzir exposição ao setor {sector.Key} ({percentage}% do portfólio, ideal < 25%)"
                );
            }

            // Adicionar recomendação de diversificação se necessário
            if (recommendations.Count == 0)
                recommendations.Add("Portfólio bem diversificado");

            return recommendations;
        }
    }
}
