using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Rules.Rewrite;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services
{
    public class SqlQueryImprovementService : ISqlQueryImprovementService
    {
        private readonly IEnumerable<ISqlRewriteRule> _rewriteRules;

        public SqlQueryImprovementService(IEnumerable<ISqlRewriteRule> rewriteRules)
        {
            _rewriteRules = GetRewriteRules();
        }

        public IList<SqlRewriteCandidate> BuildCandidates(string originalSql, IList<QueryOptimizationFinding> findings, DatabaseTypes provider)
        {
            var candidates = new List<SqlRewriteCandidate>();

            foreach (var finding in findings)
            {
                foreach (var rule in _rewriteRules)
                {
                    if (!rule.CanRewrite(finding))
                        continue;

                    var candidate = rule.TryRewrite(originalSql, finding, provider);

                    if (candidate == null)
                        continue;

                    if (string.Equals(
                            Normalize(originalSql),
                            Normalize(candidate.CandidateSql),
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    candidates.Add(candidate);

                    finding.SuggestedSql = candidate.CandidateSql;
                }
            }

            return candidates
                .GroupBy(x => Normalize(x.CandidateSql))
                .Select(x => x.First())
                .ToList();
        }

        private static string Normalize(string sql)
        {
            return string.Join(
                " ",
                sql.Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries));
        }

        private static List<ISqlRewriteRule> GetRewriteRules()
        {
            return new List<ISqlRewriteRule>()
            {
                new YearFunctionRewriteRule(),
                new ImplicitJoinRewriteRule()
            };
        }
    }
}
