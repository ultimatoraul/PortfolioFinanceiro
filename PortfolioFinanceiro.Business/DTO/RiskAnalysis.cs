namespace PortfolioFinanceiro.Business.DTO
{
    public class RiskAnalysis
    {
        public required string OverallRisk { get; set; }
        public decimal SharpeRatio { get; set; }
        public required ConcentrationRisk ConcentrationRisk { get; set; }
        public List<SectorDiversification> SectorDiversification { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class ConcentrationRisk
    {
        public required LargestPosition LargestPosition { get; set; }
        public decimal Top3Concentration { get; set; }
    }

    public class LargestPosition
    {
        public required string Symbol { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SectorDiversification
    {
        public required string Sector { get; set; }
        public decimal Percentage { get; set; }
        public required string Risk { get; set; }
    }
}
