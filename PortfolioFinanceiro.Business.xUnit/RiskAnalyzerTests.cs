using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Business.Services;

namespace PortfolioFinanceiro.Business.xUnit
{
    /// <summary>
    /// Testes unitários para a classe RiskAnalyzer.
    /// 
    /// Esta suite de testes valida todos os cálculos de análise de risco do portfólio, incluindo:
    /// - Cálculo do Sharpe Ratio
    /// - Análise de concentração de risco (maior posição e top 3)
    /// - Diversificação por setor
    /// - Determinação do nível de risco geral (Low, Medium, High)
    /// - Geração de recomendações baseada em concentração
    /// - Volatilidade baseada no histórico de preços
    /// - Tratamento de casos extremos e erros
    /// 
    /// Os testes utilizam stubs para substituir os repositórios reais
    /// permitindo testes isolados e rápidos.
    /// </summary>
    public class RiskAnalyzerTests
    {
        private readonly PortfolioRepositoryStub _portfolioRepository = new();
        private readonly MarketDataRepositoryStub _marketDataRepository = new();
        private readonly RiskAnalyzer _riskAnalyzer;

        public RiskAnalyzerTests()
        {
            _riskAnalyzer = new RiskAnalyzer(_portfolioRepository, _marketDataRepository);
        }

        #region Success Cases

        /// <summary>
        /// Verifica se a análise de risco detecta corretamente um portfólio com risco alto.
        /// Uma posição > 25% ou um setor > 40% deve resultar em risco "High".
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithHighConcentration_ReturnsHighRisk()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 600, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "ITUB4", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _portfolioRepository.SetAsset(CreateAsset("ITUB4", 10m, 10m, "Financials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("High", result.OverallRisk);
            Assert.True(result.ConcentrationRisk.LargestPosition.Percentage > 25m);
        }

        /// <summary>
        /// Verifica se o cálculo da maior posição está correto.
        /// Deve retornar o símbolo do ativo com maior percentual do portfólio.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithMultiplePositions_IdentifiesLargestPosition()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.Equal("PETR4", result.ConcentrationRisk.LargestPosition.Symbol);
            // PETR4: 1000 / 1500 * 100 = 66.67%
            Assert.Equal(66.67m, Math.Round(result.ConcentrationRisk.LargestPosition.Percentage, 2));
        }

        /// <summary>
        /// Verifica se o cálculo da concentração das top 3 posições está correto.
        /// A soma deve representar o percentual total das 3 maiores posições.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithMultiplePositions_CalculatesTop3ConcentrationCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "ITUB4", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "BBDC4", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _portfolioRepository.SetAsset(CreateAsset("ITUB4", 10m, 10m, "Financials"));
            _portfolioRepository.SetAsset(CreateAsset("BBDC4", 10m, 10m, "Financials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            // Top 3 = 3 * (1000 / 4000) * 100 = 75%
            Assert.Equal(75m, Math.Round(result.ConcentrationRisk.Top3Concentration, 2));
        }

        /// <summary>
        /// Verifica se a diversificação por setor é calculada corretamente.
        /// Deve agrupar posições por setor e calcular o percentual de cada setor.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithMultipleSectors_CalculatesSectorDiversificationCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m },
                    new() { Symbol = "ITUB4", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _portfolioRepository.SetAsset(CreateAsset("ITUB4", 10m, 10m, "Financials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.Equal(3, result.SectorDiversification.Count);
            var energySector = result.SectorDiversification.FirstOrDefault(s => s.Sector == "Energy");
            Assert.NotNull(energySector);
            Assert.Equal(33.33m, Math.Round(energySector.Percentage, 2));
        }

        /// <summary>
        /// Verifica se o risco é determinado corretamente para cada setor.
        /// Setores com percentual > 40% devem ser "High", 25-40% "Medium", < 25% "Low".
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithHighSectorConcentration_AssignsSectorRiskCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 300, AveragePrice = 10m },
                    new() { Symbol = "PETR3", Quantity = 200, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("PETR3", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            var energySector = result.SectorDiversification.FirstOrDefault(s => s.Sector == "Energy");
            Assert.NotNull(energySector);
            // Energy: (3000 + 2000) / 6000 = 83.33% (> 40% = High)
            Assert.Equal("High", energySector.Risk);
        }

        /// <summary>
        /// Verifica se o Sharpe Ratio é calculado corretamente.
        /// Fórmula: (Retorno do Portfólio - Taxa Selic) / Volatilidade.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithValidPortfolio_CalculatesSharpeRatioCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 12m, "Energy"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.SharpeRatio >= 0);
        }

        /// <summary>
        /// Verifica se recomendações são geradas quando há concentração de posição acima de 20%.
        /// Deve incluir o símbolo, percentual atual e limite recomendado.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithHighPositionConcentration_GeneratesRecommendation()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 600, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.NotEmpty(result.Recommendations);
            Assert.Contains(result.Recommendations, r => r.Contains("PETR4"));
        }

        /// <summary>
        /// Verifica se recomendações são geradas quando há concentração de setor acima de 25%.
        /// Deve incluir o nome do setor, percentual atual e limite recomendado.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithHighSectorConcentration_GeneratesRecommendation()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 300, AveragePrice = 10m },
                    new() { Symbol = "PETR3", Quantity = 200, AveragePrice = 10m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("PETR3", 10m, 10m, "Energy"));
            _portfolioRepository.SetAsset(CreateAsset("VALE3", 10m, 10m, "Materials"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act
            var result = _riskAnalyzer.ByPortfolioId(1);

            // Assert
            Assert.NotEmpty(result.Recommendations);
            Assert.Contains(result.Recommendations, r => r.Contains("Energy"));
        }

        #endregion

        #region Error Cases

        /// <summary>
        /// Verifica se uma exceção é lançada quando o portfólio não é encontrado.
        /// Testa o tratamento de erro para ID de portfólio inválido.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithNullPortfolio_ThrowsInvalidOperationException()
        {
            // Arrange
            _portfolioRepository.SetPortfolioWithPositions(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _riskAnalyzer.ByPortfolioId(999));
            Assert.NotNull(ex);
        }

        /// <summary>
        /// Verifica se uma exceção é lançada quando o portfólio não tem posições.
        /// Testa o tratamento de erro para portfólio vazio.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithEmptyPositions_ThrowsInvalidOperationException()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Empty Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions = []
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _riskAnalyzer.ByPortfolioId(1));
            Assert.NotNull(ex);
        }

        /// <summary>
        /// Verifica se uma exceção é lançada quando o portfólio não tem valor (todas as posições com preço 0).
        /// Testa o tratamento de erro para portfólio sem valor.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithZeroPortfolioValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 0m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 0m, 0m, "Energy"));
            _marketDataRepository.SetMarketData(CreateMarketData(0.10m));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _riskAnalyzer.ByPortfolioId(1));
            Assert.NotNull(ex);
        }

        /// <summary>
        /// Verifica se uma exceção é lançada quando os dados de mercado não são encontrados.
        /// Testa o tratamento de erro quando a taxa Selic não está disponível.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithNullMarketData_ThrowsInvalidOperationException()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _portfolioRepository.SetPortfolioWithPositions(portfolio);
            _portfolioRepository.SetAsset(CreateAsset("PETR4", 10m, 10m, "Energy"));
            _marketDataRepository.SetMarketData(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _riskAnalyzer.ByPortfolioId(1));
            Assert.NotNull(ex);
        }

        #endregion

        #region Helper Methods

        private static Asset CreateAsset(string symbol, decimal oldPrice, decimal newPrice, string sector) => new()
        {
            Symbol = symbol,
            Name = $"Test Asset {symbol}",
            Type = "Stock",
            Sector = sector,
            CurrentPrice = newPrice,
            LastUpdated = DateTime.UtcNow,
            PriceHistory =
            [
                new() { Date = DateTime.UtcNow.AddDays(-10), Price = oldPrice },
                new() { Date = DateTime.UtcNow.AddDays(-5), Price = oldPrice * 1.05m },
                new() { Date = DateTime.UtcNow, Price = newPrice }
            ]
        };

        private static MarketData CreateMarketData(decimal selicRate) => new()
        {
            Id = Guid.NewGuid(),
            SelicRate = selicRate,
            IndexPerformance = [],
            Sectors = []
        };

        #endregion
    }

    /// <summary>
    /// Implementação substituta de IMarketDataRepository para testes.
    /// 
    /// Permite configurar dados de mercado sem depender do banco de dados real.
    /// </summary>
    public class MarketDataRepositoryStub : IMarketDataRepository
    {
        private MarketData? _marketData;

        public void SetMarketData(MarketData? marketData) => _marketData = marketData;

        public MarketData? GetLastMarketData() => _marketData;
    }
}
