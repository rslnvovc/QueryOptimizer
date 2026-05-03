using Oracle.ManagedDataAccess.Client;
using QueryOptimizer.Providers.Oracle.Models;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Analyzing
{
    public class OracleQueryPerformanceAnalyzer : IQueryPerformanceAnalyzer
    {
        public async Task<QueryPerformanceMetrics> AnalyzeAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            QueryPerformanceMetrics result = new()
            { 
                Provider = DatabaseTypes.Oracle,
                OriginalSql = command.CommandText,
                StartedAt = DateTime.UtcNow,
                ExecutionPlanFormat = ExecutionPlanFormats.Text
            };

            var tag = $"QO_{Guid.NewGuid():N}".Substring(0, 28);
            var taggedSql = AddOracleGatherStatisticsHint(command.CommandText, tag);

            var stopwatch = Stopwatch.StartNew();

            command.CommandText = taggedSql;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            long rows = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                rows++;
            }

            result.RowsReturned = rows;

            stopwatch.Stop();

            result.FinishedAt = DateTime.UtcNow;
            result.ClientElapsedMs = stopwatch.ElapsedMilliseconds;

            var sqlInfo = await FindOracleSqlInfoAsync((OracleConnection)command.Connection, tag, cancellationToken);

            if (sqlInfo is not null)
            {
                result.DatabaseElapsedMs = sqlInfo.ElapsedTimeMicroseconds / 1000.0;
                result.CpuTimeMs = sqlInfo.CpuTimeMicroseconds / 1000.0;
                result.LogicalReads = sqlInfo.BufferGets;
                result.PhysicalReads = sqlInfo.DiskReads;

                result.ExecutionPlan = await GetOracleDisplayCursorPlanAsync(
                    (OracleConnection)command.Connection, 
                    sqlInfo.SqlId, 
                    sqlInfo.ChildNumber, 
                    cancellationToken);
            }
            else
            {
                result.Warnings.Add("Oracle SQL_ID was not found in V$SQL. Check permissions or cursor cache availability.");
            }

            return result;
        }

        public async Task<string> GetEstimatedExecutionPlanAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            var originalCommandText = command.CommandText;

            var planStatementId = $"QO_PLAN_{Guid.NewGuid():N}".Substring(0, 28);

            var explainSql = $"EXPLAIN PLAN SET STATEMENT_ID = '{planStatementId}' FOR {originalCommandText}";

            command.CommandText = explainSql;

            await command.ExecuteNonQueryAsync(cancellationToken);

            var displaySql = @"
SELECT PLAN_TABLE_OUTPUT
FROM TABLE(DBMS_XPLAN.DISPLAY(NULL, :statementId, 'TYPICAL'))
";

            await using var displayCommand = command.Connection.CreateCommand();
            displayCommand.CommandText = displaySql;
            displayCommand.CommandType = CommandType.Text;
            displayCommand.Parameters.Add(new OracleParameter("statementId", planStatementId));

            var sb = new StringBuilder();

            await using var reader = await displayCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                sb.AppendLine(reader.GetString(0));
            }

            return sb.ToString();
        }

        private static string AddOracleGatherStatisticsHint(string sql, string tag)
        {
            var trimmed = sql.TrimStart();

            if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                return "SELECT /*+ GATHER_PLAN_STATISTICS */ " +
                       $"/* {tag} */ " +
                       trimmed.Substring(6);
            }

            return $"/* {tag} */ {sql}";
        }

        private static async Task<OracleSqlInfo> FindOracleSqlInfoAsync(
            OracleConnection connection,
            string tag,
            CancellationToken cancellationToken)
        {
            var sql = @"
SELECT sql_id,
       child_number,
       elapsed_time,
       cpu_time,
       buffer_gets,
       disk_reads,
       rows_processed
FROM v$sql
WHERE sql_text LIKE :tag
ORDER BY last_active_time DESC
FETCH FIRST 1 ROWS ONLY
";

            await using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("tag", $"%{tag}%"));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new OracleSqlInfo
            {
                SqlId = reader.GetString(0),
                ChildNumber = Convert.ToInt32(reader.GetDecimal(1)),
                ElapsedTimeMicroseconds = Convert.ToInt64(reader.GetDecimal(2)),
                CpuTimeMicroseconds = Convert.ToInt64(reader.GetDecimal(3)),
                BufferGets = Convert.ToInt64(reader.GetDecimal(4)),
                DiskReads = Convert.ToInt64(reader.GetDecimal(5)),
                RowsProcessed = Convert.ToInt64(reader.GetDecimal(6))
            };
        }

        private static async Task<string> GetOracleDisplayCursorPlanAsync(
            OracleConnection connection,
            string sqlId,
            int childNumber,
            CancellationToken cancellationToken)
        {
            var sql = @"
SELECT PLAN_TABLE_OUTPUT
FROM TABLE(DBMS_XPLAN.DISPLAY_CURSOR(:sqlId, :childNumber, 'ALLSTATS LAST +IOSTATS +MEMSTATS'))
";

            await using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("sqlId", sqlId));
            command.Parameters.Add(new OracleParameter("childNumber", childNumber));

            var sb = new StringBuilder();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                sb.AppendLine(reader.GetString(0));
            }

            return sb.ToString();
        }
    }
}
