using Microsoft.EntityFrameworkCore;
using PortfolioFinanceiro.Business.Models;
using System.Text.Json;

namespace PortfolioFinanceiro.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Asset
            modelBuilder.Entity<Asset>()
                .HasKey(a => a.Symbol);

            // Configurar Portfolio
            modelBuilder.Entity<Portfolio>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Portfolio>()
                .OwnsMany(p => p.Positions);

            // Configurar Portfolio
            modelBuilder.Entity<PriceHistory>()
                .HasKey(p => p.Guid);

            // Carregar dados do SeedData.json
            SeedDatabase(modelBuilder);
        }

        private void SeedDatabase(ModelBuilder modelBuilder)
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

                // Seed Assets
                if (root.TryGetProperty("assets", out var assetsElement))
                {
                    var assets = JsonSerializer.Deserialize<List<Asset>>(
                        assetsElement.GetRawText(),
                        options
                    );

                    if (assets != null)
                        modelBuilder.Entity<Asset>().HasData(assets);
                }

                // Seed Portfolios
                if (root.TryGetProperty("portfolios", out var portfoliosElement))
                {
                    var portfolios = JsonSerializer.Deserialize<List<Portfolio>>(
                        portfoliosElement.GetRawText(),
                        options
                    );

                    if (portfolios != null)
                    {
                        modelBuilder.Entity<Portfolio>().HasData(
                            portfolios.Select((p, index) => new { p, Id = index + 1 })
                                .Select(x => new Portfolio
                                {
                                    Id = x.Id,
                                    Name = x.p.Name,
                                    UserId = x.p.UserId,
                                    TotalInvestment = x.p.TotalInvestment,
                                    CreatedAt = x.p.CreatedAt
                                })
                        );
                    }
                }
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
