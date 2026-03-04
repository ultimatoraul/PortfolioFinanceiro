using System.Text.Json.Serialization;

namespace PortfolioFinanceiro.Business.Models
{
    public class Portfolio
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required string UserId { get; set; }
        public decimal TotalInvestment { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Position> Positions { get; set; } = [];
    }

    public class Position
    {
        [JsonPropertyName("assetSymbol")]
        public required string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TargetAllocation { get; set; }
        public DateTime LastTransaction { get; set; }
    }
}
