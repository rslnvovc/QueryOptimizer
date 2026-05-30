using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class ConnectionStringModel
    {
        public DatabaseTypes Provider { get; set; }
        public string? ServerName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? DatabaseName { get; set; }
        public string? Host { get; set; }
        public int? Port { get; set; }
        public string? ServiceName { get; set; }
    }
}
