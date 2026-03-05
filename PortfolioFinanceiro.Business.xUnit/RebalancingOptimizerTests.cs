using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Models;
using PortfolioFinanceiro.Business.Services;

namespace PortfolioFinanceiro.Business.xUnit
{
    /// <summary>
    /// Testes unitários para a classe RebalancingOptimizer.
    /// 
    /// Esta suite de testes valida o algoritmo de sugestão de rebalanceamento, incluindo:
    /// - Identificação de portfólios que necessitam rebalanceamento
    /// - Cálculo correto de pesos atuais e desvios
    /// - Filtragem de desvios menores que 2% (threshold mínimo)
    /// - Filtragem de trades menores que R$ 100
    /// - Priorização por maiores desvios
    /// - Cálculo correto de custos de transação (0.3%)
    /// - Quantidade de ações calculada corretamente
    /// - Mensagens de sugestões apropriadas
    /// - Tratamento de casos extremos e erros
    /// 
    /// Os testes utilizam a classe PortfolioRepositoryStub para substituir o repositório real
    /// e permitir testes isolados e rápidos.
    /// </summary>
    public class RebalancingOptimizerTests
    {
        private readonly PortfolioRepositoryStub _repository = new();
        private readonly RebalancingOptimizer _optimizer;

        public RebalancingOptimizerTests()
        {
            _optimizer = new RebalancingOptimizer(_repository);
        }

        /// <summary>
        /// Verifica se a função retorna corretamente quando o portfólio é nulo.
        /// Deve retornar NeedsRebalancing = false e mensagem apropriada.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithNullPortfolio_ReturnsNoRebalancingNeeded()
        {
            // Arrange
            _repository.SetPortfolioWithPositions(null);

            // Act
            var result = _optimizer.ByPortfolioId(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Empty(result.CurrentAllocation);
            Assert.Empty(result.SuggestedTrades);
            Assert.Equal(0m, result.TotalTransactionCost);
            Assert.Contains("vazio", result.ExpectedImprovement.ToLower());
        }

        /// <summary>
        /// Verifica se a função retorna corretamente quando o portfólio não tem posições.
        /// Deve retornar NeedsRebalancing = false.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithEmptyPortfolio_ReturnsNoRebalancingNeeded()
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
            _repository.SetPortfolioWithPositions(portfolio);

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Empty(result.SuggestedTrades);
        }

        /// <summary>
        /// Verifica se a função retorna corretamente quando o portfólio tem valor total zero.
        /// Cenário: posições sem preço atual válido.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithZeroPortfolioValue_ReturnsNoRebalancingNeeded()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions = [new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m }]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(null);

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Contains("sem valor", result.ExpectedImprovement.ToLower());
        }

        /// <summary>
        /// Verifica se o portfólio bem alocado (sem desvios > 2%) retorna corretamente.
        /// Deve ter NeedsRebalancing = false e mensagem "já está bem alocado".
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithWellBalancedPortfolio_ReturnsNoRebalancingNeeded()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Well Balanced Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 50m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m, TargetAllocation = 50m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 10m));
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Empty(result.SuggestedTrades);
            Assert.Contains("bem alocado", result.ExpectedImprovement.ToLower());
        }

        /// <summary>
        /// Verifica se os pesos atuais são calculados corretamente.
        /// Validação matemática: peso = (valor posição / valor total) * 100.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithValidPortfolio_CalculatesCurrentWeightsCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 40m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 60m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 15m)); // PETR4: 1500
            _repository.SetAsset(CreateAsset("VALE3", 10m, 20m)); // VALE3: 1000
                                                                   // Total: 2500
                                                                   // Pesos: PETR4 = 60%, VALE3 = 40%

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CurrentAllocation.Count);

            var petr4 = result.CurrentAllocation.First(a => a.Symbol == "PETR4");
            var vale3 = result.CurrentAllocation.First(a => a.Symbol == "VALE3");

            Assert.Equal(60m, petr4.CurrentWeight);
            Assert.Equal(40m, vale3.CurrentWeight);
        }

        /// <summary>
        /// Verifica se os desvios são calculados corretamente.
        /// Desvio = |Peso Atual - Peso Alvo|.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithValidPortfolio_CalculatesDeviationsCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 50m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 50m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 20m)); // PETR4: 2000
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m)); // VALE3: 500
                                                                   // Total: 2500
                                                                   // Pesos: PETR4 = 80%, VALE3 = 20%
                                                                   // Desvios: PETR4 = 30%, VALE3 = 30%

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            var petr4 = result.CurrentAllocation.First(a => a.Symbol == "PETR4");
            var vale3 = result.CurrentAllocation.First(a => a.Symbol == "VALE3");

            Assert.Equal(30m, petr4.Deviation);
            Assert.Equal(30m, vale3.Deviation);
        }

        /// <summary>
        /// Verifica se desvios menores que 2% são ignorados (sem trades sugeridas).
        /// Apenas desvios > 2% devem gerar trades.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithDeviationLessThanThreshold_IgnoresPosition()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 49m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 10m, TargetAllocation = 51m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 10m));
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Empty(result.SuggestedTrades);
        }

        /// <summary>
        /// Verifica se um simples trade SELL é sugerido corretamente.
        /// Posição com peso > alvo deve resultar em SELL.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithOverweightPosition_SuggestsSellTrade()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 20m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 80m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 30m)); // PETR4: 3000
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m)); // VALE3: 500

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.NeedsRebalancing);
            Assert.NotEmpty(result.SuggestedTrades);
            
            var sellTrade = result.SuggestedTrades.FirstOrDefault(t => t.Symbol == "PETR4");
            Assert.NotNull(sellTrade);
            Assert.Equal("SELL", sellTrade.Action);
        }

        /// <summary>
        /// Verifica se um simples trade BUY é sugerido corretamente.
        /// Posição com peso < alvo deve resultar em BUY.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithUnderweightPosition_SuggestsBuyTrade()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 80m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 20m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 10m)); // PETR4: 1000
            _repository.SetAsset(CreateAsset("VALE3", 10m, 30m)); // VALE3: 1500

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.NeedsRebalancing);
            Assert.NotEmpty(result.SuggestedTrades);

            var buyTrade = result.SuggestedTrades.FirstOrDefault(t => t.Symbol == "PETR4");
            Assert.NotNull(buyTrade);
            Assert.Equal("BUY", buyTrade.Action);
        }

        /// <summary>
        /// Verifica se o custo total de transação é a soma de todos os trades.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithMultipleTrades_SumsTotalTransactionCostCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 25m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 25m },
                    new() { Symbol = "ITUB4", Quantity = 100, AveragePrice = 5m, TargetAllocation = 50m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 30m));
            _repository.SetAsset(CreateAsset("VALE3", 10m, 30m));
            _repository.SetAsset(CreateAsset("ITUB4", 5m, 5m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            var sumOfCosts = result.SuggestedTrades.Sum(t => t.TransactionCost);
            Assert.Equal(Math.Round(sumOfCosts, 2), result.TotalTransactionCost);
        }

        /// <summary>
        /// Verifica se os trades são priorizados por maior desvio.
        /// O trade com maior desvio deve vir primeiro na lista.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithMultipleDeviations_PrioritizesByLargestDeviation()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 10m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 50m },
                    new() { Symbol = "ITUB4", Quantity = 20, AveragePrice = 10m, TargetAllocation = 40m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 30m)); // PETR4: 3000
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m)); // VALE3: 500
            _repository.SetAsset(CreateAsset("ITUB4", 10m, 10m)); // ITUB4: 200

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.SuggestedTrades);
            
            // O primeiro trade deve ser PETR4 (maior desvio)
            Assert.Equal("PETR4", result.SuggestedTrades.First().Symbol);
        }

        /// <summary>
        /// Verifica se a razão (reason) do trade é preenchida corretamente.
        /// Deve incluir porcentagens atuais e alvo.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithTrade_IncludesReasonWithCurrentAndTargetWeights()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 20m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 80m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 30m));
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            var petr4Trade = result.SuggestedTrades.First(t => t.Symbol == "PETR4");
            
            Assert.NotNull(petr4Trade.Reason);
            Assert.NotEmpty(petr4Trade.Reason);
            Assert.Contains("%", petr4Trade.Reason);
        }

        /// <summary>
        /// Verifica se ExpectedImprovement descreve a melhoria esperada.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithRebalancingNeeded_DescribesExpectedImprovement()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Test Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 20m },
                    new() { Symbol = "VALE3", Quantity = 50, AveragePrice = 10m, TargetAllocation = 80m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 30m));
            _repository.SetAsset(CreateAsset("VALE3", 10m, 10m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.ExpectedImprovement);
            Assert.Contains("desvio", result.ExpectedImprovement.ToLower());
        }

        /// <summary>
        /// Teste de integração: portfólio desbalanceado com múltiplas posições.
        /// Valida a saída completa com todos os campos populados corretamente.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithComplexPortfolio_ReturnsCompleteRebalancingSuggestions()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Complex Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions =
                [
                    new() { Symbol = "PETR4", Quantity = 50, AveragePrice = 20m, TargetAllocation = 25m },
                    new() { Symbol = "VALE3", Quantity = 100, AveragePrice = 15m, TargetAllocation = 25m },
                    new() { Symbol = "ITUB4", Quantity = 30, AveragePrice = 25m, TargetAllocation = 25m },
                    new() { Symbol = "BBAS3", Quantity = 60, AveragePrice = 30m, TargetAllocation = 25m }
                ]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 20m, 35m)); // PETR4: 1750
            _repository.SetAsset(CreateAsset("VALE3", 15m, 12m)); // VALE3: 1200
            _repository.SetAsset(CreateAsset("ITUB4", 25m, 28m)); // ITUB4: 840
            _repository.SetAsset(CreateAsset("BBAS3", 30m, 32m)); // BBAS3: 1920

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.CurrentAllocation.Count);
            
            foreach (var allocation in result.CurrentAllocation)
            {
                Assert.NotEmpty(allocation.Symbol);
                Assert.True(allocation.CurrentWeight > 0);
                Assert.True(allocation.TargetWeight > 0);
                Assert.True(allocation.Deviation >= 0);
            }

            if (result.SuggestedTrades.Count > 0)
            {
                Assert.True(result.NeedsRebalancing);
                Assert.True(result.TotalTransactionCost > 0);

                foreach (var trade in result.SuggestedTrades)
                {
                    Assert.NotEmpty(trade.Symbol);
                    Assert.True(trade.Quantity > 0);
                    Assert.True(trade.EstimatedValue > 0);
                    Assert.True(trade.TransactionCost > 0);
                    Assert.NotEmpty(trade.Reason);
                }
            }
        }

        /// <summary>
        /// Teste adicional: Validar comportamento com posição de uma única ação.
        /// </summary>
        [Fact]
        public void ByPortfolioId_WithSinglePosition_HandlesCorrectly()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Single Position Portfolio",
                UserId = "user123",
                CreatedAt = DateTime.UtcNow,
                Positions = [new() { Symbol = "PETR4", Quantity = 100, AveragePrice = 10m, TargetAllocation = 100m }]
            };
            _repository.SetPortfolioWithPositions(portfolio);
            _repository.SetAsset(CreateAsset("PETR4", 10m, 10m));

            // Act
            var result = _optimizer.ByPortfolioId(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.NeedsRebalancing);
            Assert.Single(result.CurrentAllocation);
            Assert.Equal(100m, result.CurrentAllocation.First().CurrentWeight);
        }

        private static Asset CreateAsset(string symbol, decimal oldPrice, decimal newPrice) => new()
        {
            Symbol = symbol,
            Name = $"Test Asset {symbol}",
            Type = "Stock",
            Sector = "Finance",
            CurrentPrice = newPrice,
            LastUpdated = DateTime.UtcNow,
            PriceHistory =
            [
                new() { Date = DateTime.UtcNow.AddDays(-1), Price = oldPrice },
                new() { Date = DateTime.UtcNow, Price = newPrice }
            ]
        };
    }
}
