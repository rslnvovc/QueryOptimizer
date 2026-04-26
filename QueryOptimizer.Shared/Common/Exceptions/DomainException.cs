using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
