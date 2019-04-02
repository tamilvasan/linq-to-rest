using System;

namespace Messerli.LinqToRest
{
    public class QueryProviderBuilderException : Exception
    {
        public QueryProviderBuilderException()
        {
        }

        public QueryProviderBuilderException(string message)
            : base(message)
        {
        }

        public QueryProviderBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}