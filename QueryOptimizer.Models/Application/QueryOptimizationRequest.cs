using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class QueryOptimizationRequest
    {
        public int UserId { get; set; }
        public ConnectionStringModel ConnectionStringModel { get; set; } = new();
        public string Sql { get; set; } = default!;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
