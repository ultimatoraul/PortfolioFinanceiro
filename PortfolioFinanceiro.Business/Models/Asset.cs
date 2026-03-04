namespace PortfolioFinanceiro.Business.Models
{
    public class Asset
    {
        public required string Symbol { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Sector { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PriceHistory> PriceHistory { get; set; } = [];
    }

    public class PriceHistory
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public required string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
