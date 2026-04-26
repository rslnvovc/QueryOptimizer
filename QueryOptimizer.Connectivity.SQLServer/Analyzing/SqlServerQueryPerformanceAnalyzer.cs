using Microsoft.Data.SqlClient;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Providers.SQLServer.Analyzing
{
    public class SqlServerQueryPerformanceAnalyzer : IQueryPerformanceAnalyzer
    {
        public async Task<QueryPerformanceMetrics> AnalyzeAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            QueryPerformanceMetrics result = new()
            { 
                Provider = DatabaseTypes.SqlServer,
                OriginalSql = command.CommandText,
                StartedAt = DateTime.UtcNow,
                ExecutionPlanFormat = ExecutionPlanFormats.Xml
            };

            var messages = new List<string>();

            if (command.Connection is SqlConnection sqlConnection)
            {
                sqlConnection.InfoMessage += (_, e) =>
                {
                    messages.Add(e.Message);
                };
            }

            var sqlToExecute = $@"
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
SET STATISTICS XML ON;

{command.CommandText};

SET STATISTICS XML OFF;
SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
";

            command.CommandText = sqlToExecute;

            var stopwatch = Stopwatch.StartNew();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            do
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.FieldCount == 1)
                    {
                        var value = reader.GetValue(0)?.ToString();

                        if (!string.IsNullOrWhiteSpace(value) &&
                            value.TrimStart().StartsWith("<ShowPlanXML", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ExecutionPlan = value;
                        }
                    }
                }
            } while (await reader.NextResultAsync(cancellationToken));

            stopwatch.Stop();

            result.FinishedAt = DateTime.UtcNow;
            result.ClientElapsedMs = stopwatch.ElapsedMilliseconds;

            ParseSqlServerMessages(messages, result);

            foreach (var message in messages)
            {
                result.RawMetrics.TryAdd(Guid.NewGuid().ToString(), message);
            }

            return result;
        }

        public async Task<string> GetEstimatedExecutionPlanAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Connection is not SqlConnection sqlConnection)
                throw new InvalidOperationException("Command connection must be SqlConnection.");

            var originalCommandText = command.CommandText;
            var transaction = command.Transaction as SqlTransaction;

            try
            {
                await ExecuteSqlServerSessionCommandAsync(
                    sqlConnection,
                    transaction,
                    "SET SHOWPLAN_XML ON",
                    cancellationToken);

                command.CommandText = originalCommandText;

                var plans = new StringBuilder();

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                do
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (reader.FieldCount != 1)
                            continue;

                        var value = reader.GetValue(0)?.ToString();

                        if (!string.IsNullOrWhiteSpace(value) &&
                            value.TrimStart().StartsWith("<ShowPlanXML", StringComparison.OrdinalIgnoreCase))
                        {
                            plans.AppendLine(value);
                        }
                    }
                }
                while (await reader.NextResultAsync(cancellationToken));

                return plans.ToString();
            }
            finally
            {
                command.CommandText = originalCommandText;

                await ExecuteSqlServerSessionCommandAsync(
                    sqlConnection,
                    transaction,
                    "SET SHOWPLAN_XML OFF",
                    cancellationToken);
            }
        }

        private static async Task ExecuteSqlServerSessionCommandAsync(
            SqlConnection connection,
            SqlTransaction? transaction,
            string sql,
            CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static void ParseSqlServerMessages(List<string> messages, QueryPerformanceMetrics metrics)
        {
            foreach (var message in messages)
            {
                var logicalReadsMatch = Regex.Match(
                    message,
                    @"logical reads (?<value>\d+)",
                    RegexOptions.IgnoreCase
                    );

                if (logicalReadsMatch.Success)
                {
                    metrics.LogicalReads = (metrics.LogicalReads ?? 0) + long.Parse(logicalReadsMatch.Groups["value"].Value);
                }

                var physicalReadsMatch = Regex.Match(
                    message,
                    @"physical reads (?<value>\d+)",
                    RegexOptions.IgnoreCase
                    );

                if (physicalReadsMatch.Success)
                {
                    metrics.PhysicalReads = (metrics.PhysicalReads ?? 0) + long.Parse(physicalReadsMatch.Groups["value"].Value);
                }

                var cpuTimeMatch = Regex.Match(
                    message,
                    @"CPU time = (?<value>\d+) ms",
                    RegexOptions.IgnoreCase
                    );

                if (cpuTimeMatch.Success)
                {
                    metrics.CpuTimeMs = double.Parse(cpuTimeMatch.Groups["value"].Value);
                }

                var elapsedTimeMatch = Regex.Match(
                    message,
                    @"elapsed time = (?<value>\d+) ms",
                    RegexOptions.IgnoreCase
                    );

                if (elapsedTimeMatch.Success)
                {
                    metrics.DatabaseElapsedMs = double.Parse(elapsedTimeMatch.Groups["value"].Value);
                }
            }
        }
    }
}
