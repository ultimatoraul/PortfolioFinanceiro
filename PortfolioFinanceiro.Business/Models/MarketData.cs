namespace PortfolioFinanceiro.Business.Models
{
    internal class MarketData
    {
        public decimal SelicRate { get; set; }
        public Dictionary<string, IndexPerformance> IndexPerformance { get; set; } = [];
        public List<SectorData> Sectors { get; set; } = [];
    }

    internal class IndexPerformance
    {
        public decimal CurrentValue { get; set; }
        public decimal DailyChange { get; set; }
        public decimal MonthlyChange { get; set; }
        public decimal YearToDate { get; set; }
    }

    internal class SectorData
    {
        public required string Name { get; set; }
        public decimal AverageReturn { get; set; }
        public decimal Volatility { get; set; }
        public List<string> Assets { get; set; } = [];
    }
}
