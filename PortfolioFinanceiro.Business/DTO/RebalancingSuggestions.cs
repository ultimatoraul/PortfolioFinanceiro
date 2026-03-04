namespace PortfolioFinanceiro.Business.DTO
{
    public class RebalancingSuggestions
    {
        public bool NeedsRebalancing { get; set; }
        public List<Allocation> CurrentAllocation { get; set; } = [];
        public List<SuggestedTrade> SuggestedTrades { get; set; } = [];
        public decimal TotalTransactionCost { get; set; }
        public required string ExpectedImprovement { get; set; }
    }

    public class Allocation
    {
        public required string Symbol { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal TargetWeight { get; set; }
        public decimal Deviation { get; set; }
    }

    public class SuggestedTrade
    {
        public required string Symbol { get; set; }
        public required string Action { get; set; }
        public int Quantity { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal TransactionCost { get; set; }
        public required string Reason { get; set; }
    }
}
