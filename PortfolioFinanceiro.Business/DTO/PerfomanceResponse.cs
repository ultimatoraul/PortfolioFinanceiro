namespace PortfolioFinanceiro.Business.DTO
{
    public class PerfomanceResponse
    {
        private decimal _totalInvestment;
        private decimal _currentValue;  
        private decimal _totalReturn;
        private decimal _totalReturnAmount;
        private decimal _annualizedReturn;
        private decimal? _volatility;
        public decimal TotalInvestment 
        { 
            get => _totalInvestment;
            set => _totalInvestment = Math.Round(value, 2);
        }

        public decimal CurrentValue 
        { 
            get => _currentValue;
            set => _currentValue = Math.Round(value, 2);
        }

        public decimal TotalReturn 
        { 
            get => _totalReturn;
            set => _totalReturn = Math.Round(value, 2);
        }

        public decimal TotalReturnAmount 
        { 
            get => _totalReturnAmount;
            set => _totalReturnAmount = Math.Round(value, 2);
        }

        public decimal AnnualizedReturn 
        { 
            get => _annualizedReturn;
            set => _annualizedReturn = Math.Round(value, 2);
        }

        public decimal? Volatility 
        { 
            get => _volatility;
            set => _volatility = value != null ? Math.Round((decimal)value, 2) : null;
        }

        public List<PositionPerformance> PositionsPerformance { get; set; } = [];
    }

    public class PositionPerformance
    {
        private decimal _investedAmount;
        private decimal _currentValue;
        private decimal _return;
        private decimal _weight;

        public required string Symbol { get; set; }
        public decimal InvestedAmount 
        { 
            get => _investedAmount;
            set => _investedAmount = Math.Round(value, 2);
        }
        public decimal CurrentValue 
        { 
            get => _currentValue;
            set => _currentValue = Math.Round(value, 2);
        }
        public decimal Return 
        { 
            get => _return;
            set => _return = Math.Round(value, 2);
        }
        public decimal Weight 
        { 
            get => _weight;
            set => _weight = Math.Round(value, 2);
        }
    }
}
