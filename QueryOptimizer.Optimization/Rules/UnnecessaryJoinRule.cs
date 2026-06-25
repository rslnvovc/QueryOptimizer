using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Helpers;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Common.Models.Queries;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Optimization.Rules
{
    public class UnnecessaryJoinRule : IOptimizationRule
    {
        public string Rule => "UNNECESSARY_JOIN";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            if (string.IsNullOrWhiteSpace(originalSql))
                yield break;

            if (!IsSelectQuery(originalSql))
                yield break;

            var joins = SqlJoinHelper.ExtractJoins(originalSql);

            foreach (var join in joins)
            {
                if (join.JoinType.Contains("CROSS", StringComparison.OrdinalIgnoreCase))
                    continue;

                var sqlWithoutJoin = SqlJoinHelper.RemoveJoin(originalSql, join);

                if (SqlJoinHelper.ContainsAliasReference(sqlWithoutJoin, join.TableAlias))
                    continue;

                yield return new QueryOptimizationFinding()
                {
                    RuleCode = Rule,
                    Title = "Можливо зайве з'єднання таблиці",
                    Description =
                        $"У запиті виявлено JOIN з таблицею '{join.TableName}' з alias '{join.TableAlias}', " +
                        "але цей alias не використовується поза власною умовою ON. " +
                        "Це може свідчити про зайве з’єднання таблиці.",
                    Recommendation =
                        "Перевірте, чи це з’єднання справді потрібне для логіки запиту. " +
                        "Якщо таблиця не використовується для вибірки, фільтрації або перевірки наявності пов’язаних записів, " +
                        "JOIN можна прибрати для спрощення запиту та потенційного зменшення навантаження на СУБД.",
                    SuggestedSql = sqlWithoutJoin,
                    AffectedObject = $"{join.TableName} {join.TableAlias}",
                    AffectedNodeType = "JOIN",
                    Severity = FindingSeverity.Low,
                    Confidence = GetConfidence(join),
                    AdaptiveConfidence = GetConfidence(join)
                };
            }
        }

        private static bool IsSelectQuery(string sql)
        {
            return Regex.IsMatch(sql.TrimStart(), @"^SELECT\b", RegexOptions.IgnoreCase);
        }

        private static double GetConfidence(SqlJoinInfo join)
        {
            if (join.JoinType.Contains("LEFT", StringComparison.OrdinalIgnoreCase))
                return 0.60;

            if (join.JoinType.Contains("INNER", StringComparison.OrdinalIgnoreCase))
                return 0.45;

            return 0.40;
        }
    }
}
