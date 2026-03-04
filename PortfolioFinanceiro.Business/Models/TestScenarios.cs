namespace PortfolioFinanceiro.Business.Models
{
    public class TestScenarios
    {
        public List<TestScenario> Scenarios { get; set; } = [];
    }

    public class TestScenario
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Portfolio? Portfolio { get; set; }
        public ExpectedResults? ExpectedResults { get; set; }
    }

    public class ExpectedResults
    {
        public decimal TotalValue { get; set; }
        public Dictionary<string, decimal> Allocations { get; set; } = [];
        public bool RebalancingNeeded { get; set; }
        public List<SuggestedAction> SuggestedActions { get; set; } = [];
    }

    public class SuggestedAction
    {
        public string? Action { get; set; }
        public string? Asset { get; set; }
        public decimal Value { get; set; }
    }
}

