using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueryOptimizer.Bll.Services.Abstractions;

namespace QueryOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet("UserHistory/{userId:int}")]
        public async Task<IActionResult> GetQueryAnalysisRunsByUser(
            int userId,
            CancellationToken cancellationToken)
        {
            var result = await _historyService.GetQueryAnalysisRunsByUserAsync(
                userId,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("Analysis/{analysisRunId:int}")]
        public async Task<IActionResult> GetAnalysisDetails(
            int analysisRunId,
            CancellationToken cancellationToken)
        {
            var result = await _historyService.GetAnalysisDetailsAsync(
                analysisRunId,
                cancellationToken);

            return Ok(result);
        }
    }
}
