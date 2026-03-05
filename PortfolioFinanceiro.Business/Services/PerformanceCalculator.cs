using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Repositories;
using PortfolioFinanceiro.Business.Interfaces.Services;

namespace PortfolioFinanceiro.Business.Services
{
    public class PerformanceCalculator : IPerformanceCalculator
    {
        private readonly IPortfolioRepository _repository;

        public PerformanceCalculator(IPortfolioRepository repository)
        {
            _repository = repository;
        }

        public Perfomance ToAnalyze(long id)
        {
            throw new NotImplementedException();
        }
    }
}
