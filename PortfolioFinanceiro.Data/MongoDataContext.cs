using MongoDB.Driver;
using PortfolioFinanceiro.Business.Models;
using System.Text.Json;

namespace PortfolioFinanceiro.Data
{
    public class MongoDataContext
    {
        private readonly IMongoDatabase _database;

        public IMongoCollection<Asset> Assets => _database.GetCollection<Asset>("assets");
        public IMongoCollection<Portfolio> Portfolios => _database.GetCollection<Portfolio>("portfolios");
        public IMongoCollection<MarketData> MarketData => _database.GetCollection<MarketData>("marketData");
        public IMongoCollection<PriceHistory> PriceHistory => _database.GetCollection<PriceHistory>("priceHistory");

        public MongoDataContext(IMongoClient mongoClient, string databaseName)
        {
            _database = mongoClient.GetDatabase(databaseName);
        }

        public async Task InitializeAsync()
        {
            await SeedDatabaseAsync();
        }

        private async Task SeedDatabaseAsync()
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

                using var document = System.Text.Json.JsonDocument.Parse(json);
                var root = document.RootElement;

                // Seed Assets
                if (root.TryGetProperty("assets", out var assetsElement) && 
                    await Assets.EstimatedDocumentCountAsync() == 0)
                {
                    var assets = System.Text.Json.JsonSerializer.Deserialize<List<Asset>>(
                        assetsElement.GetRawText(),
                        options
                    );

                    if (assets?.Any() == true)
                        await Assets.InsertManyAsync(assets);
                }

                // Seed Portfolios
                if (root.TryGetProperty("portfolios", out var portfoliosElement) && 
                    await Portfolios.EstimatedDocumentCountAsync() == 0)
                {
                    var portfolios = System.Text.Json.JsonSerializer.Deserialize<List<Portfolio>>(
                        portfoliosElement.GetRawText(),
                        options
                    );

                    if (portfolios?.Any() == true)
                    {
                        var portfoliosToInsert = portfolios
                            .Select((p, index) => new Portfolio
                            {
                                Id = index + 1,
                                Name = p.Name,
                                UserId = p.UserId,
                                TotalInvestment = p.TotalInvestment,
                                CreatedAt = p.CreatedAt
                            })
                            .ToList();

                        await Portfolios.InsertManyAsync(portfoliosToInsert);
                    }
                }

                // Seed MarketData
                if (root.TryGetProperty("marketData", out var marketDataElement) && 
                    await MarketData.EstimatedDocumentCountAsync() == 0)
                {
                    var selicRate = marketDataElement.GetProperty("selicRate").GetDecimal();

                    var indexPerformanceList = new List<IndexPerformance>();
                    if (marketDataElement.TryGetProperty("indexPerformance", out var indexPerfElement))
                    {
                        foreach (var property in indexPerfElement.EnumerateObject())
                        {
                            var indexPerf = System.Text.Json.JsonSerializer.Deserialize<IndexPerformance>(
                                property.Value.GetRawText(),
                                options
                            );
                            if (indexPerf != null)
                            {
                                indexPerf.Index = property.Name;
                                indexPerformanceList.Add(indexPerf);
                            }
                        }
                    }

                    var sectorsList = new List<SectorData>();
                    if (marketDataElement.TryGetProperty("sectors", out var sectorsElement))
                    {
                        sectorsList = System.Text.Json.JsonSerializer.Deserialize<List<SectorData>>(
                            sectorsElement.GetRawText(),
                            options
                        ) ?? [];
                    }

                    var marketData = new MarketData
                    {
                        Id = Guid.NewGuid(),
                        SelicRate = selicRate,
                        IndexPerformance = indexPerformanceList,
                        Sectors = sectorsList
                    };

                    await MarketData.InsertOneAsync(marketData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar SeedData.json: {ex.Message}");
            }
        }
    }
}