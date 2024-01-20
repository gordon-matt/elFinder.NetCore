using System.Runtime.Serialization;
using System;

namespace elFinder.NetCore.Exceptions
{
    public class InvalidPathException : ApplicationException
    {
        public InvalidPathException()
        {
        }

        public InvalidPathException(string message)
            : base(message)
        {
        }

        public InvalidPathException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}