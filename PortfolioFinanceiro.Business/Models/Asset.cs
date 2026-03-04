namespace PortfolioFinanceiro.Business.Models
{
    internal class Asset
    {
        public required string Symbol { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Sector { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PriceHistory> PriceHistory { get; set; } = [];
    }

    internal class PriceHistory
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
