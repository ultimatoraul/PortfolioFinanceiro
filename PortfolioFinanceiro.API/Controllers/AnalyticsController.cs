using Microsoft.AspNetCore.Mvc;
using PortfolioFinanceiro.API.Utils;
using PortfolioFinanceiro.Business.DTO;
using PortfolioFinanceiro.Business.Interfaces.Services;

namespace PortfolioFinanceiro.API.Controllers
{
    [Route("api/portfolios")]
    [ApiController]
    public class AnalyticsController(IPerformanceCalculator performanceCalculator, IRebalancingOptimizer rebalancingOptimizer, IRiskAnalyzer riskAnalyzer) : ControllerBase
    {
        private readonly IPerformanceCalculator _performanceCalculatorService = performanceCalculator;
        private readonly IRebalancingOptimizer _rebalancingOptimizerService = rebalancingOptimizer;
        private readonly IRiskAnalyzer _riskAnalyzerService = riskAnalyzer;


        [HttpGet("{id}/performance-analysis")]
        public ActionResult<PerfomanceResult> GetPerformanceAnalysis(string id)
        {
            try
            {
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                PerfomanceResult result = _performanceCalculatorService.ToAnalyze(NumberHelper.StringToLong(id));
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
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                RebalancingSuggestions result = _rebalancingOptimizerService.Rebalancing(NumberHelper.StringToLong(id));
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
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                RiskAnalysis result = _riskAnalyzerService.RiskAnalysis(NumberHelper.StringToLong(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
