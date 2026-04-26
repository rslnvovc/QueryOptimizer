using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.Metrics
{
    public class QueryPlanNodeMetric
    {
        public string? NodeType { get; set; }

        public string? RelationName { get; set; }

        public double? EstimatedCost { get; set; }

        public double? ActualTimeMs { get; set; }

        public long? EstimatedRows { get; set; }

        public long? ActualRows { get; set; }

        public long? LogicalReads { get; set; }

        public long? PhysicalReads { get; set; }

        public long? MemoryKb { get; set; }
    }
}
