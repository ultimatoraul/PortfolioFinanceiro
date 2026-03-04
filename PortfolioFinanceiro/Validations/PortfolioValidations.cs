using PortfolioFinanceiro.API.Utils;

namespace PortfolioFinanceiro.API.Validations
{
    public class PortfolioValidations
    {
        static internal void IdIsNumeric(string id)
        {
            if (!NumberHelper.IsNumeric(id))
                throw new ArgumentException($"The number ({id}) isn't numeric");
        }

    }
}
