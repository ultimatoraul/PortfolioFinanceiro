
namespace PortfolioFinanceiro.Business.Utils
{
    internal static class FinancialCalculator
    {
        public static decimal DailyReturnPercentual(decimal todayPrice, decimal previousPrice) => ((todayPrice - previousPrice) / previousPrice) * 100;

        public static decimal ReturnOfInvestmentPercentual(decimal currentValue, decimal investedAmount) => (currentValue - investedAmount) * 100 / investedAmount;

        public static decimal WeightPercentual(decimal singleValue, decimal totalValue) => singleValue / totalValue * 100;
    }
}
