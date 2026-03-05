using Microsoft.AspNetCore.Mvc;
using PortfolioFinanceiro.API.Utils;
using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Services;

namespace PortfolioFinanceiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController(IPerformanceCalculator performanceCalculator, IRebalancingOptimizer rebalancingOptimizer, IRiskAnalyzer riskAnalyzer) : ControllerBase
    {
        private readonly IPerformanceCalculator _performanceCalculatorService = performanceCalculator;
        private readonly IRebalancingOptimizer _rebalancingOptimizerService = rebalancingOptimizer;
        private readonly IRiskAnalyzer _riskAnalyzerService = riskAnalyzer;


        [HttpGet("{id}/performance-analysis")]
        public ActionResult<Perfomance> GetPerformanceAnalysis(long id)
        {
            try
            {
                //if (!NumberHelper.IsNumeric(id))
                //    throw new ArgumentException($"The number ({id}) isn't numeric");

                Perfomance result = _performanceCalculatorService.ToAnalyze(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/rebalacing")]
        public ActionResult<RebalancingSuggestions> GetRebalancing(string id)
        {
            try
            {
                if (!NumberHelper.IsNumeric(id))
                    throw new ArgumentException($"The number ({id}) isn't numeric");

                RebalancingSuggestions result = _rebalancingOptimizerService.Rebalancing(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/risk-analysis")]
        public ActionResult<RiskAnalysis> GetRiskAnalysis(string id)
        {
            try
            {
                if (!NumberHelper.IsNumeric(id))
                    throw new ArgumentException($"The number ({id}) isn't numeric");

                RiskAnalysis result = _riskAnalyzerService.RiskAnalysis(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
