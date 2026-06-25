using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.Queries
{
    public class SqlJoinInfo
    {
        public string JoinType { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string TableAlias { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
