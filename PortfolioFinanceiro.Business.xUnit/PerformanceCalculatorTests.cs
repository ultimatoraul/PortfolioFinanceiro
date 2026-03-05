using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Business.Services;

namespace PortfolioFinanceiro.Business.xUnit
{
    /// <summary>
    /// Testes unitários para a classe PerformanceCalculator.
    /// 
    /// Esta suite de testes valida todos os cálculos de performance do portfólio, incluindo:
    /// - Investimento total e valor atual
    /// - Retorno percentual por posição e retorno total do portfólio
    /// - Peso de cada posição (asset allocation)
    /// - Volatilidade baseada no histórico de preços
    /// - Precisão decimal (máximo 2 casas decimais)
    /// - Tratamento de casos extremos e erros
    /// 
    /// Os testes utilizam a classe PortfolioRepositoryStub para substituir o repositório real
    /// e permitir testes isolados e rápidos.
    /// </summary>
    public class PerformanceCalculatorTests
    {
        private readonly PortfolioRepositoryStub _repository = new();
        private readonly PerformanceCalculator _calculator;

        public PerformanceCalculatorTests()
        {
            _calculator = new PerformanceCalculator(_repository);
        }

        /// <summary>
        /// Verifica se o cálculo do investimento total está correto.
        /// O investimento total deve ser a soma de (quantidade * preço médio) de todas as posições.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithValidPortfolio_CalculatesTotalInvestmentCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 28.75m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000m, result.TotalInvestment); // 100 * 10
        }

        /// <summary>
        /// Verifica se o cálculo do valor atual do portfólio está correto.
        /// O valor atual deve ser a soma de (quantidade * preço atual) de todas as posições.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithValidPortfolio_CalculatesCurrentValueCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 30m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000m, result.CurrentValue); // 100 * 30
        }

        /// <summary>
        /// Verifica se o cálculo do retorno percentual de uma posição está correto.
        /// A fórmula é: ((valor atual - valor investido) / valor investido) * 100.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithValidPortfolio_CalculatesPositionReturnPercentageCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 35m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.PositionsPerformance);
            var position = result.PositionsPerformance[0];

            // Expected return: (3500 - 1000) / 1000 * 100 = 250%
            Assert.Equal(250m, position.Return);
        }

        /// <summary>
        /// Verifica se o cálculo de retorno negativo está correto.
        /// Testa o cenário onde o preço atual da posição é menor que o preço de custo.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithNegativeReturn_CalculatesPositionReturnCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 5m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            var position = result.PositionsPerformance[0];

            // Expected return: (500 - 1000) / 1000 * 100 = -50%
            Assert.True(position.Return < 0);
        }

        /// <summary>
        /// Verifica se o cálculo do peso de cada posição no portfólio está correto.
        /// O peso de uma posição é: (valor atual da posição / valor total do portfólio) * 100.
        /// A soma de todos os pesos deve totalizar 100%.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithMultiplePositions_CalculatesWeightCorrectly()
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
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 20m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 15m));
            _repository.SetAsset(CreateAsset("VALE3", 20m, 25m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.Equal(2, result.PositionsPerformance.Count);

            // PETR4: 1500 / 3250 * 100 = 46.15%
            // VALE3: 1250 / 3250 * 100 = 38.46%
            decimal totalWeight = result.PositionsPerformance.Sum(p => p.Weight);
            Assert.Equal(100m, Math.Round(totalWeight, 2));
        }

        /// <summary>
        /// Verifica se o cálculo do retorno total do portfólio está correto.
        /// O retorno total é calculado como: ((valor atual total - investimento total) / investimento total) * 100.
        /// Também valida se o valor absoluto retornado (TotalReturnAmount) está correto.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithValidPortfolio_CalculatesTotalReturnCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            // Invested: 100 * 10 = 1000, Current: 100 * 12 = 1200
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 12m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalReturn > 0);
            Assert.Equal(200m, result.TotalReturnAmount);
            // (200 / 1000) * 100 = 20%
            Assert.Equal(20m, result.TotalReturn);
        }

        /// <summary>
        /// Verifica se uma exceção é lançada quando o portfólio não é encontrado.
        /// Testa o tratamento de erro para ID de portfólio inválido.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithNullPortfolio_ThrowsInvalidOperationException()
        {
            // Arrange
            _repository.SetPortfolioWithPositions(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _calculator.ToAnalyze(999));
            Assert.NotNull(ex);
        }

        /// <summary>
        /// Verifica se posições com ativo não encontrado são ignoradas corretamente.
        /// O cálculo deve continuar sem falhar, apenas pulando a posição inválida.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithNullAsset_SkipsPosition()
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
                    new() { Symbol = "INVALID", Quantity = 100, AveragePrice = 10m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(null);

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.PositionsPerformance);
        }

        /// <summary>
        /// Verifica o tratamento correto de posições com quantidade zero.
        /// O retorno deve ser zero para evitar divisão por zero ou comportamento indefinido.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithZeroInvestedAmount_PositionReturnIsZero()
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
                    new() { Symbol = "PETR4", Quantity = 0, AveragePrice = 10m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 15m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.PositionsPerformance);
            Assert.Equal(0m, result.PositionsPerformance[0].Return);
        }

        /// <summary>
        /// Verifica se a volatilidade retorna null quando não há histórico de preços suficiente.
        /// A volatilidade requer pelo menos 2 preços para calcular o desvio padrão.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithInsufficientPriceHistory_VolatilityIsNull()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 30m));
            _repository.SetPriceHistory("PETR4",
            [
                new() { Date = DateTime.UtcNow, Price = 28.50m }
            ]);

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Volatility);
        }

        /// <summary>
        /// Verifica se a volatilidade é calculada corretamente com histórico de preços válido.
        /// A volatilidade é o desvio padrão dos retornos diários percentuais.
        /// Deve ser um valor positivo quando há variação nos preços.
        /// </summary>
        [Fact]
        public void ToAnalyze_WithValidPriceHistory_CalculatesVolatilityCorrectly()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 30m));
            _repository.SetPriceHistory("PETR4",
            [
                new() { Date = DateTime.UtcNow.AddDays(-5), Price = 25m },
                new() { Date = DateTime.UtcNow.AddDays(-4), Price = 26m },
                new() { Date = DateTime.UtcNow.AddDays(-3), Price = 27m },
                new() { Date = DateTime.UtcNow.AddDays(-2), Price = 28m },
                new() { Date = DateTime.UtcNow.AddDays(-1), Price = 29m },
                new() { Date = DateTime.UtcNow, Price = 30m }
            ]);

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Volatility);
            Assert.True(result.Volatility > 0);
        }

        /// <summary>
        /// Teste Extra para validar a precisão decimal dos cálculos de performance.
        /// Verifica se todos os valores decimais estão limitados a no máximo 2 casas decimais.
        /// Isso garante precisão financeira adequada sem valores muito pequenos que não fazem sentido.
        /// </summary>
        [Fact]
        public void ToAnalyze_DecimalPropertiesAreLimitedToTwoDecimalPlaces()
        {
            // Arrange
            var portfolio = CreatePortfolioWithPositions(1);
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 28.50m, 28.75m));

            // Act
            var result = _calculator.ToAnalyze(1);

            // Assert
            Assert.NotNull(result);
            AssertTwoDecimalPlaces(result.TotalInvestment);
            AssertTwoDecimalPlaces(result.CurrentValue);
            AssertTwoDecimalPlaces(result.TotalReturn);
            AssertTwoDecimalPlaces(result.TotalReturnAmount);

            foreach (var position in result.PositionsPerformance)
            {
                AssertTwoDecimalPlaces(position.InvestedAmount);
                AssertTwoDecimalPlaces(position.CurrentValue);
                AssertTwoDecimalPlaces(position.Return);
                AssertTwoDecimalPlaces(position.Weight);
            }
        }

        private static Portfolio CreatePortfolioWithPositions(long id) => new()
        {
            Id = id,
            Name = "Test Portfolio",
            UserId = "user123",
            CreatedAt = DateTime.UtcNow.AddDays(-100),
            Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m }
                ]
        };

        private static Asset CreateAsset(string symbol, decimal oldPrice, decimal newPrice) => new()
        {
            Symbol = symbol,
            Name = $"Test Asset {symbol}",
            Type = "Stock",
            Sector = "Energy",
            CurrentPrice = newPrice,
            LastUpdated = DateTime.UtcNow,
            PriceHistory =
                [
                    new() { Date = DateTime.UtcNow.AddDays(-1), Price = oldPrice },
                    new() { Date = DateTime.UtcNow, Price = newPrice }
                ]
        };

        private static void AssertTwoDecimalPlaces(decimal value)
        {
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
            Assert.True(decimalPlaces <= 2, $"Value {value} has {decimalPlaces} decimal places, expected max 2");
        }
    }

    // Implementação do IPortfolioRepository para Testes
    public class PortfolioRepositoryStub : IPortfolioRepository
    {
        private Portfolio? _portfolio;
        private readonly Dictionary<string, Asset> _assets = [];
        private readonly Dictionary<string, List<PriceHistory>> _priceHistories = [];

        public void SetPortfolioWithPositions(Portfolio? portfolio) => _portfolio = portfolio;
        public void SetAsset(Asset? asset)
        {
            if (asset == null)
                _assets.Clear();
            else
                _assets[asset.Symbol] = asset;
        }
        public void SetPriceHistory(string symbol, List<PriceHistory> history) => _priceHistories[symbol] = history;

        public Portfolio? GetPortfolioWithPositions(long id) => _portfolio;
        public Asset? GetAssetWithPriceHistory(string symbol) => _assets.TryGetValue(symbol, out Asset? value) ? value : null;
        public List<PriceHistory> GetPriceHistoryBySymbol(string symbol) => _priceHistories.TryGetValue(symbol, out List<PriceHistory>? value) ? value : [];
    }
}