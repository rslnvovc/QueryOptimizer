using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.Caching
{
    public class MetricsCash
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();

        public QueryPerformanceMetrics Metrics { get; set; } = new QueryPerformanceMetrics();
    }
}
