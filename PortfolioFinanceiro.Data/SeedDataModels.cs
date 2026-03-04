namespace PortfolioFinanceiro.Data
{
    /// <summary>
    /// DTOs para desserializar dados do SeedData.json
    /// </summary>
    public static class SeedDataModels
    {
        public class AssetSeedData
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Sector { get; set; }
            public decimal CurrentPrice { get; set; }
            public DateTime LastUpdated { get; set; }
            public List<PriceHistorySeedData> PriceHistory { get; set; } = [];
        }

        public class PriceHistorySeedData
        {
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
        }

        public class PortfolioSeedData
        {
            public string Name { get; set; }
            public string UserId { get; set; }
            public decimal TotalInvestment { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<PositionSeedData> Positions { get; set; } = [];
        }

        public class PositionSeedData
        {
            public string AssetSymbol { get; set; }
            public int Quantity { get; set; }
            public decimal AveragePrice { get; set; }
            public decimal TargetAllocation { get; set; }
            public DateTime LastTransaction { get; set; }
        }

        public class MarketDataSeedData
        {
            public decimal SelicRate { get; set; }
            public Dictionary<string, IndexPerformance> IndexPerformance { get; set; }
            public List<SectorData> Sectors { get; set; }
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
            public string Name { get; set; }
            public decimal AverageReturn { get; set; }
            public decimal Volatility { get; set; }
            public List<string> Assets { get; set; }
        }

        public class TestScenarioSeedData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public PortfolioSeedData Portfolio { get; set; }
            public Dictionary<string, object> ExpectedResults { get; set; }
        }
    }
}
