using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.Models.Analyzing;
using QueryOptimizer.Models.Analyzing.DTO;
using QueryOptimizer.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IAnalyzingRepository _analyzingRepository;

        public HistoryService(IAnalyzingRepository analyzingRepository)
        {
            this._analyzingRepository = analyzingRepository;
        }

        public async Task<IList<QueryAnalysisRuns>> GetQueryAnalysisRunsByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _analyzingRepository.GetQueryAnalysisRunsByUserAsync(
                userId,
                cancellationToken);
        }

        public async Task<QueryAnalysisFullResultModel> GetAnalysisDetailsAsync(int analysisRunId, CancellationToken cancellationToken = default)
        {
            return await _analyzingRepository.GetQueryAnalysisRunFullAsync(
                analysisRunId,
                cancellationToken);
        }
    }
}
