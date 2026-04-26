using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Exceptions.Database
{
    public class DatabaseNameIsNotExistsException : DomainException
    {
        public DatabaseNameIsNotExistsException()
            : base("Database name is not missing. Please, check your connection string and make sure that everything is correct")
        { }
    }
}
