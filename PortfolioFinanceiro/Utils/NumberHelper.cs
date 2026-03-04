namespace PortfolioFinanceiro.API.Utils
{
    public class NumberHelper
    {
        static internal decimal StringToDecimal(string strNumber)
        {

            if (decimal.TryParse(strNumber,
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out decimal result))
            {
                return result;
            }

            throw new ArgumentException("Invalid strNumber");
        }

        static internal bool IsNumeric(string strNumber)
        {
            if (decimal.TryParse(strNumber,
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out decimal result))
            {
                return true;
            }

            return false;
        }
    }
}
