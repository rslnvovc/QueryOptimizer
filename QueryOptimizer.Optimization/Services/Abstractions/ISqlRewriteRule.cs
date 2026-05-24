using QueryOptimizer.Models.Application;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services.Abstractions
{
    public interface ISqlRewriteRule
    {
        string RuleCode { get; }

        bool CanRewrite(QueryOptimizationFinding finding);

        SqlRewriteCandidate? TryRewrite(
            string originalSql,
            QueryOptimizationFinding finding,
            DatabaseTypes provider
            );
    }
}
