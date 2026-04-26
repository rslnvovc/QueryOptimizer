using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Exceptions.Database
{
    public class NotValidConnectionStringException : DomainException
    {
        public NotValidConnectionStringException()
            : base("Your connection string is not valid. Please, check your connection string and make sure that login or password was not empty or invalid.")
        { }
    }
}
