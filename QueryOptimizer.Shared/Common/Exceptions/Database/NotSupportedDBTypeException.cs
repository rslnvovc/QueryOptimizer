using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Exceptions.Database
{
    public class NotSupportedDBTypeException : DomainException
    {
        public NotSupportedDBTypeException() : base("DBMS that you want to use is not allowed in this application!")
        { }
    }
}
