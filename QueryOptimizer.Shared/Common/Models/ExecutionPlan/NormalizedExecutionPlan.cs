using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.ExecutionPlan
{
    public class NormalizedExecutionPlan
    {
        public DatabaseTypes Provider { get; set; }

        public string RawPlan { get; set; } = default!;

        public List<ExecutionPlanNode> Nodes { get; set; } = new();

        public double? TotalCost { get; set; }

        public double? TotalExecutionTimeMs { get; set; }

        public long? TotalLogicalReads { get; set; }

        public long? TotalPhysicalReads { get; set; }
    }
}
