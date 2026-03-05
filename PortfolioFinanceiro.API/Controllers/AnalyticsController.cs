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
        public ActionResult<PerfomanceResponse> GetPerformanceAnalysis(string id)
        {
            try
            {
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                PerfomanceResponse result = _performanceCalculatorService.ByPortfolioId(NumberHelper.StringToLong(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/rebalacing")]
        public ActionResult<RebalancingSuggestionsResponse> GetRebalancing(string id)
        {
            try
            {
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                RebalancingSuggestionsResponse result = _rebalancingOptimizerService.ByPortfolioId(NumberHelper.StringToLong(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/risk-analysis")]
        public ActionResult<RiskAnalysisResponse> GetRiskAnalysis(string id)
        {
            try
            {
                if (!NumberHelper.IsLongType(id))
                    throw new ArgumentException(PortfolioAPIResource.PortfolioIdInvalid);

                RiskAnalysisResponse result = _riskAnalyzerService.ByPortfolioId(NumberHelper.StringToLong(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
