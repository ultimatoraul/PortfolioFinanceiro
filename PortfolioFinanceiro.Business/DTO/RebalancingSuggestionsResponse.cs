namespace PortfolioFinanceiro.Business.DTO
{
    public class RebalancingSuggestionsResponse
    {
        private decimal _totalTransactionCost;

        public bool NeedsRebalancing { get; set; }
        public List<Allocation> CurrentAllocation { get; set; } = [];
        public List<SuggestedTrade> SuggestedTrades { get; set; } = [];
        public decimal TotalTransactionCost
        {
            get => _totalTransactionCost;
            set => _totalTransactionCost = Math.Round(value, 2);
        }
        public required string ExpectedImprovement { get; set; }
    }

    public class Allocation
    {
        private decimal _currentWeight;
        private decimal _targetWeight;
        private decimal _deviation;

        public required string Symbol { get; set; }
        public decimal CurrentWeight
        {
            get => _currentWeight;
            set => _currentWeight = Math.Round(value, 2);
        }
        public decimal TargetWeight
        {
            get => _targetWeight;
            set => _targetWeight = Math.Round(value, 2);
        }
        public decimal Deviation
        {
            get => _deviation;
            set => _deviation = Math.Round(value, 2);
        }
    }

    public class SuggestedTrade
    {
        private decimal _estimatedValue;
        private decimal _transactionCost;

        public required string Symbol { get; set; }
        public required string Action { get; set; }
        public int Quantity { get; set; }
        public decimal EstimatedValue
        {
            get => _estimatedValue;
            set => _estimatedValue = Math.Round(value, 2);
        }
        public decimal TransactionCost
        {
            get => _transactionCost;
            set => _transactionCost = Math.Round(value, 2);
        }
        public required string Reason { get; set; }
    }
}
