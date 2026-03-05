namespace PortfolioFinanceiro.Business.DTO
{
    public class RiskAnalysisResult
    {
        public required string OverallRisk { get; set; }
        private decimal _sharpeRatio;
        public decimal SharpeRatio
        {
            get => _sharpeRatio;
            set => _sharpeRatio = Math.Round(value, 2);
        }
        public required ConcentrationRisk ConcentrationRisk { get; set; }
        public List<SectorDiversification> SectorDiversification { get; set; } = [];
        public List<string> Recommendations { get; set; } = [];
    }

    public class ConcentrationRisk
    {
        private decimal _top3Concentration;
        public decimal Top3Concentration
        {
            get => _top3Concentration;
            set => _top3Concentration = Math.Round(value, 2);
        }
        public required LargestPosition LargestPosition { get; set; }
    }

    public class LargestPosition
    {
        public required string Symbol { get; set; }
        private decimal _percentage;
        public decimal Percentage
        {
            get => _percentage;
            set => _percentage = Math.Round(value, 2);
        }
    }

    public class SectorDiversification
    {
        public required string Sector { get; set; }
        public required string Risk { get; set; }

        private decimal _percentage;
        public decimal Percentage
        {
            get => _percentage;
            set => _percentage = Math.Round(value, 2);
        }
    }
}
