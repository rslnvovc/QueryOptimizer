using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Shared.Common.Helpers
{
    public static class QueryHashHelper
    {
        public static string ComputeHash(string sql)
        {
            var normalizedSql = NormalizeSql(sql);

            using var sha256 = System.Security.Cryptography.SHA256.Create();

            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedSql));

            return Convert.ToHexString(bytes);
        }

        public static string NormalizeSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return string.Empty;

            var result = sql.Trim();

            result = Regex.Replace(result, @"--.*?$", string.Empty, RegexOptions.Multiline);
            result = Regex.Replace(result, @"/\*[\s\S]*?\*/", string.Empty);

            result = Regex.Replace(result, @"\s+", " ");

            result = Regex.Replace(result, @"N?'[^']*'", "?", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\b\d+(\.\d+)?\b", "?");

            return result.Trim().ToLowerInvariant();
        }
    }
}
