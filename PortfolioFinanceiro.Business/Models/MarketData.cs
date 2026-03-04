namespace PortfolioFinanceiro.Business.Models
{
    public class MarketData
    {
        public decimal SelicRate { get; set; }
        public Dictionary<string, IndexPerformance> IndexPerformance { get; set; } = [];
        public List<SectorData> Sectors { get; set; } = [];
    }

    public class IndexPerformance
    {
        public decimal CurrentValue { get; set; }
        public decimal DailyChange { get; set; }
        public decimal MonthlyChange { get; set; }
        public decimal YearToDate { get; set; }
    }

    public class SectorData
    {
        public required string Name { get; set; }
        public decimal AverageReturn { get; set; }
        public decimal Volatility { get; set; }
        public List<string> Assets { get; set; } = [];
    }
}
