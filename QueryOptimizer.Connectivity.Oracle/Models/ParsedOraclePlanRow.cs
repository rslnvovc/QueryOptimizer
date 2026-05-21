using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Models
{
    public sealed class ParsedOraclePlanRow
    {
        public int Id { get; set; }

        public int Depth { get; set; }

        public ExecutionPlanNode Node { get; set; } = default!;
    }
}
