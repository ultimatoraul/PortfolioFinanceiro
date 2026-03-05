using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Interfaces.Services;

namespace PortfolioFinanceiro.Business.Services
{
    public class PerformanceCalculator(IPortfolioRepository repository) : IPerformanceCalculator
    {
        private readonly IPortfolioRepository _repository = repository;

        public Perfomance ToAnalyze(long id)
        {
            throw new NotImplementedException();
        }
    }
}
