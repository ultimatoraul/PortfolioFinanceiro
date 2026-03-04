using Microsoft.AspNetCore.Mvc;
using PortfolioFinanceiro.Business.Interfaces;
using PortfolioFinanceiro.API.Utils;
using PortfolioFinanceiro.Business.DTO;

namespace PortfolioFinanceiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfoliosController(IPortfolioService service) : ControllerBase
    {
        private readonly IPortfolioService _service = service;

        [HttpGet("{id}/performance")]
        public ActionResult<Perfomance> GetPerformance(string id)
        {
            try
            {
                if (!NumberHelper.IsNumeric(id))
                    throw new ArgumentException($"The number ({id}) isn't numeric");

                Perfomance result = _service.Performance(id);
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

                RiskAnalysis result = _service.RiskAnalysis(id);
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

                RebalancingSuggestions result = _service.Rebalancing(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
