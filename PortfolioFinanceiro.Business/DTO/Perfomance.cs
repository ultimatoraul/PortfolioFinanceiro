namespace PortfolioFinanceiro.Business.DTO
{
    public class Perfomance
    {
        public decimal TotalInvestment { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal AnnualizedReturn { get; set; }
        public decimal Volatility { get; set; }
        public List<PositionPerformance> PositionsPerformance { get; set; } = new();
    }

    public class PositionPerformance
    {
        public required string Symbol { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Return { get; set; }
        public decimal Weight { get; set; }
    }
}
