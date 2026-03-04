namespace PortfolioFinanceiro.Business.Models
{
    internal class TestScenarios
    {
        public List<TestScenario> Scenarios { get; set; } = [];
    }

    internal class TestScenario
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Portfolio Portfolio { get; set; }
        public required ExpectedResults ExpectedResults { get; set; }
    }

    internal class ExpectedResults
    {
        public decimal TotalValue { get; set; }
        public Dictionary<string, decimal> Allocations { get; set; } = [];
        public bool RebalancingNeeded { get; set; }
        public List<SuggestedAction> SuggestedActions { get; set; } = [];
    }

    internal class SuggestedAction
    {
        public required string Action { get; set; }
        public required string Asset { get; set; }
        public decimal Value { get; set; }
    }
}

