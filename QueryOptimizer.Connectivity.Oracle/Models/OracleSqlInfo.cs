using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Models
{
    public class OracleSqlInfo
    {
        public string SqlId { get; set; } = default!;

        public int ChildNumber { get; set; }

        public long ElapsedTimeMicroseconds { get; set; }

        public long CpuTimeMicroseconds { get; set; }

        public long BufferGets { get; set; }

        public long DiskReads { get; set; }

        public long RowsProcessed { get; set; }
    }
}
