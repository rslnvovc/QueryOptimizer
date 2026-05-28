using QueryOptimizer.Models.Analyzing;
using QueryOptimizer.Models.Analyzing.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services.Abstractions
{
    public interface IHistoryService
    {
        Task<IList<QueryAnalysisRuns>> GetQueryAnalysisRunsByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<QueryAnalysisFullResultModel> GetAnalysisDetailsAsync(
            int analysisRunId,
            CancellationToken cancellationToken = default
            );
    }
}
