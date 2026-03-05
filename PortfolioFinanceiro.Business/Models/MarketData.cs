namespace PortfolioFinanceiro.Business.Models
{
    public class MarketData
    {
        public Guid Id { get; set; }
        public decimal SelicRate { get; set; }
        public List<IndexPerformance> IndexPerformance { get; set; } = [];
        public List<SectorData> Sectors { get; set; } = [];
    }

    public class IndexPerformance
    {
        public Guid Id { get; set; }
        public Guid MarketDataId { get; set; }
        public string? Index { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal DailyChange { get; set; }
        public decimal MonthlyChange { get; set; }
        public decimal YearToDate { get; set; }
    }

    public class SectorData
    {
        public Guid Id { get; set; }
        public Guid MarketDataId { get; set; }
        public string? Name { get; set; }
        public decimal AverageReturn { get; set; }
        public decimal Volatility { get; set; }
        public List<string> Assets { get; set; } = [];
    }
}
