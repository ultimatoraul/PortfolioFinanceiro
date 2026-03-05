using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Models;
using System.Text.Json;

namespace PortfolioFinanceiro.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<MarketData> MarketData { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Asset
            modelBuilder.Entity<Asset>()
                .HasKey(a => a.Symbol);

           // Configurar PriceHistory
            modelBuilder.Entity<PriceHistory>()
                .HasKey(ph => ph.Id);

            // Configurar Portfolio
            modelBuilder.Entity<Portfolio>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Portfolio>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Portfolio>()
                .OwnsMany(p => p.Positions);

            // Configurar MarketData
            modelBuilder.Entity<MarketData>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<MarketData>()
                .OwnsMany(m => m.Sectors);

            modelBuilder.Entity<MarketData>()
                .OwnsMany(m => m.IndexPerformance);

            // Carregar dados do SeedData.json
            SeedDatabase(modelBuilder);
        }

        private static void SeedDatabase(ModelBuilder modelBuilder)
        {
            try
            {
                var seedDataPath = Path.Combine(AppContext.BaseDirectory, "SeedData.json");

                if (!File.Exists(seedDataPath))
                    return;

                var json = File.ReadAllText(seedDataPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                #region Seed Assets
                if (root.TryGetProperty("assets", out var assetsElement))
                {
                    var assetsObj = JsonSerializer.Deserialize<List<Asset>>(
                        assetsElement.GetRawText(),
                        options
                    );

                    if (assetsObj != null && assetsObj.Count > 0)
                        modelBuilder.Entity<Asset>().HasData(assetsObj);
                }
                #endregion

                #region Seed Portfolios
                if (root.TryGetProperty("portfolios", out var portfoliosElement))
                {
                    var portfoliosObj = JsonSerializer.Deserialize<List<Portfolio>>(
                        portfoliosElement.GetRawText(),
                        options
                    );

                    if (portfoliosObj != null)
                    {
                        var portfoliosList = new List<Portfolio>();
                        var positionsList = new List<Position>();
                        int portfolioIndex = 1;

                        foreach (var p in portfoliosObj)
                        {
                            var newPortfolio = new Portfolio
                            {
                                Id = portfolioIndex,
                                Name = p.Name,
                                UserId = p.UserId,
                                TotalInvestment = p.TotalInvestment,
                                CreatedAt = p.CreatedAt
                            };

                            if (p.Positions != null)
                            {
                                int positionIndex = 1;
                                foreach (var pos in p.Positions)
                                {
                                    var newPosition = new Position
                                    {
                                        Id = (portfolioIndex * 100) + positionIndex,
                                        PortfolioId = portfolioIndex,
                                        Symbol = pos.Symbol,
                                        Quantity = pos.Quantity,
                                        AveragePrice = pos.AveragePrice,
                                        TargetAllocation = pos.TargetAllocation,
                                        LastTransaction = pos.LastTransaction
                                    };
                                    positionsList.Add(newPosition);
                                    positionIndex++;
                                }
                            }

                            portfoliosList.Add(newPortfolio);
                            portfolioIndex++;
                        }

                        modelBuilder.Entity<Portfolio>()
                            .HasData(portfoliosList);

                        if (positionsList.Count > 0)
                        {
                            modelBuilder.Entity<Portfolio>()
                                .OwnsMany(p => p.Positions)
                                .HasData(positionsList);
                        }
                    }
                }
                #endregion

                #region Seed MarketData
                if (root.TryGetProperty("marketData", out var marketDataElement))
                {
                    var marketDataObj = marketDataElement.GetProperty("selicRate");
                    var selicRate = marketDataObj.GetDecimal();

                    var marketDataGuid = Guid.NewGuid();

                    modelBuilder.Entity<MarketData>().HasData(
                        new MarketData
                        {
                            Id = marketDataGuid,
                            SelicRate = selicRate
                        }
                    );

                    var indexPerformanceList = new List<IndexPerformance>();
                    if (marketDataElement.TryGetProperty("indexPerformance", out var indexPerfElement))
                    {
                        foreach (var property in indexPerfElement.EnumerateObject())
                        {
                            var indexPerf = JsonSerializer.Deserialize<IndexPerformance>(
                                property.Value.GetRawText(),
                                options
                            );
                            if (indexPerf != null)
                            {
                                indexPerf.Id = Guid.NewGuid();
                                indexPerf.Index = property.Name;
                                indexPerf.MarketDataId = marketDataGuid;
                                indexPerformanceList.Add(indexPerf);
                            }
                        }
                    }

                    if (indexPerformanceList.Count != 0)
                    {
                        modelBuilder.Entity<MarketData>()
                            .OwnsMany(m => m.IndexPerformance)
                            .HasData(indexPerformanceList);
                    }

                    var sectorsList = new List<SectorData>();
                    if (marketDataElement.TryGetProperty("sectors", out var sectorsElement))
                    {
                        sectorsList = JsonSerializer.Deserialize<List<SectorData>>(
                            sectorsElement.GetRawText(),
                            options
                        ) ?? [];

                        foreach (var sector in sectorsList)
                        {
                            sector.Id = Guid.NewGuid();
                            sector.MarketDataId = marketDataGuid;
                        }
                    }

                    if (sectorsList.Count != 0)
                    {
                        modelBuilder.Entity<MarketData>()
                            .OwnsMany(m => m.Sectors)
                            .HasData(sectorsList);
                    }
                }
                #endregion

                #region Seed PriceHistory
                var priceHistoryList = new List<PriceHistory>();
                if (root.TryGetProperty("priceHistory", out var priceHistoryElement))
                {
                    foreach (var property in priceHistoryElement.EnumerateObject())
                    {
                        var priceHistoriesObj = JsonSerializer.Deserialize<List<PriceHistory>>(
                            property.Value.GetRawText(),
                            options
                        );

                        foreach (var priceHistoryObj in priceHistoriesObj!)
                        {
                            if (priceHistoryObj != null)
                            {
                                priceHistoryObj.Id = Guid.NewGuid();
                                priceHistoryObj.Symbol = property.Name;
                                priceHistoryList.Add(priceHistoryObj);
                            }
                        }
                    }
                }
                if (priceHistoryList.Any())
                {
                    modelBuilder.Entity<PriceHistory>()
                        .HasData(priceHistoryList);
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar SeedData.json: {ex.Message}");
            }
        }

        public async Task InitializeAsync()
        {
            await Database.EnsureCreatedAsync();
        }
    }
}
