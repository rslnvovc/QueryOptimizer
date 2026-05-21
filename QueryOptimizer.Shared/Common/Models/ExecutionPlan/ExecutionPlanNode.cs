using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.ExecutionPlan
{
    public class ExecutionPlanNode
    {
        public string NodeType { get; set; } = default!;

        public string NormalizedNodeType { get; set; } = default!;

        public string? ObjectName { get; set; } = default!;

        public string? IndexName { get; set; } = default!;

        public string? Predicate { get; set; } = default!;

        public string? JoinType { get; set; } = default!;

        public double? EstimatedCost { get; set; }

        public long? EstimatedRows { get; set; }

        public long? ActualRows { get; set; }

        public double? ActualTimeMs { get; set; }

        public long? LogicalReads { get; set; }

        public long? PhysicalReads { get; set; }

        public List<ExecutionPlanNode> ChildrenNode { get; set; } = new();
    }
}
