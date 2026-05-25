using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.Models.Application;

namespace QueryOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueryOptimizerController : ControllerBase
    {
        private readonly IQueryOptimizationWorkflowService _queryOptimizationWorkflowService;

        public QueryOptimizerController(IQueryOptimizationWorkflowService queryOptimizationWorkflowService)
        {
            this._queryOptimizationWorkflowService = queryOptimizationWorkflowService;
        }

        [HttpPost("Analyze")]
        public async Task<IActionResult> Analyze(QueryOptimizationRequest request, CancellationToken cancellationToken)
        {
            var result = await _queryOptimizationWorkflowService.AnalyzeAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
