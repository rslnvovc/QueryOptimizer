using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Optimization.Rules
{
    public class SelectStarRule : IOptimizationRule
    {
        public string Rule => "SELECT_STAR";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            if (!Regex.IsMatch(originalSql, @"\bselect\s+\*", RegexOptions.IgnoreCase))
                yield break;

            yield return new QueryOptimizationFinding()
            { 
                RuleCode = Rule,
                Title = "Використання SELECT *",
                Description = "Використання SELECT * може призводити до неефективного виконання запиту, оскільки повертаються всі колонки, включно з тими, які можуть бути непотрібними.\nЦе може збільшувати використання I/O та пам'яті.",
                Recommendation = "Замість SELECT * вкажіть лише ті колонки, які необхідні для запиту.",
                Severity = FindingSeverity.Medium,
                Confidence = 0.95
            };
        }
    }
}
