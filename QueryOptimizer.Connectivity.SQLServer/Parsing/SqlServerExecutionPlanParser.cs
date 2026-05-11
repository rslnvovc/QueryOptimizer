using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace QueryOptimizer.Providers.SQLServer.Parsing
{
    public class SqlServerExecutionPlanParser : IExecutionPlanParser
    {
        private static readonly XNamespace ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            var result = new NormalizedExecutionPlan()
            {
                Provider = DatabaseTypes.SqlServer,
                RawPlan = executionPlan.ExecutionPlan ?? string.Empty,
                TotalExecutionTimeMs = executionPlan.DatabaseElapsedMs,
                TotalLogicalReads = executionPlan.LogicalReads,
                TotalPhysicalReads = executionPlan.PhysicalReads,
            };

            if (string.IsNullOrEmpty(result.RawPlan))
                return result;


            return result;
        }

        private static void ParseTotalCost(XDocument document, NormalizedExecutionPlan result)
        {
            var statementCosts = document
                .Descendants(ShowPlanNamespace + "StmtSimple")
                .Select(x => GetNullableDoubleAttribute(x, "StatementSubTreeCost"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (statementCosts.Count > 0)
            {
                result.TotalCost = statementCosts.Sum();
                return;
            }

            var rootRelOpCost = document
                .Descendants(ShowPlanNamespace + "RelOp")
                .Select(x => GetNullableDoubleAttribute(x, "EstimatedTotalSubtreeCost"))
                .FirstOrDefault(x => x.HasValue);

            result.TotalCost = rootRelOpCost;
        }

        private static void ParseMemoryGrant(XDocument document, NormalizedExecutionPlan result)
        { 
            var memoryGrantInfo = document
                .Descendants(ShowPlanNamespace + "MemoryGrantInfo")
                .FirstOrDefault();

            if (memoryGrantInfo is null)
                return;

            result.RequestedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "RequestedMemory");
            result.GrantedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "GrantedMemory");
            result.MaxUsedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "MaxUsedMemory");
        }

        private static string? GetStringAttribute(XElement element, string attributeName)
        {
            return element.Attribute(attributeName)?.Value;
        }

        private static double? GetNullableDoubleAttribute(XElement element, string attributeName)
        {
            var value = element.Attribute(attributeName)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static long? GetNullableLongAttribute(XElement element, string attributeName)
        {
            var value = element.Attribute(attributeName)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static long? GetNullableLongFromDoubleAttribute(XElement element, string attributeName)
        {
            var value = GetNullableDoubleAttribute(element, attributeName);

            if (!value.HasValue)
                return null;

            return Convert.ToInt64(Math.Round(value.Value));
        }

        private static string? CleanSqlServerIdentifier(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Trim();
        }
    }
}
