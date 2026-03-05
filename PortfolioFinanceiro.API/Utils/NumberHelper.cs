namespace PortfolioFinanceiro.API.Utils
{
    public class NumberHelper
    {
        static internal long StringToLong(string strNumber)
        {

            if (long.TryParse(strNumber,
                out long result))
            {
                return result;
            }

            throw new ArgumentException("Invalid strNumber");
        }


        static internal bool IsLongType(string strNumber)
        {
            if (long.TryParse(strNumber,
                out long result))
            {
                return true;
            }

            return false;
        }
    }
}
