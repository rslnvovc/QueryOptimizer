using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor.Helpers
{
    public static class SqlDescription
    {
        public static string PrepareSqlDescription(string sql, Dictionary<string, object> parameters)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(sql);

            if (parameters != null && parameters.Count > 0)
            {
                stringBuilder.Append(" ");
                int index = 0;
                var count = parameters.Count;
                foreach (var pair in parameters)
                {
                    var valueStr = "NULL";
                    if (pair.Value != null)
                    {
                        var table = pair.Value as DataTable;
                        if (table != null)
                        {
                            using (StringWriter sw = new StringWriter())
                            {
                                table.WriteXml(sw);
                                valueStr = sw.ToString();
                            }
                        }
                        else if (pair.Value is string)
                        {
                            valueStr = $"'{pair.Value}'";
                        }
                        else if (pair.Value is DateTime)
                        {
                            valueStr = $"'{(DateTime)pair.Value:yyyy-MM-dd HH:mm:ss}'";
                        }
                        else
                        {
                            valueStr = pair.Value.ToString();
                        }
                    }
                    stringBuilder.AppendFormat("{0} = {1}", pair.Key.IndexOf('@') != 0 ? "@" + pair.Key : pair.Key, valueStr);
                    if (index != count - 1)
                        stringBuilder.Append(", ");
                    index++;
                }
            }
            return stringBuilder.ToString();
        }
    }
}
