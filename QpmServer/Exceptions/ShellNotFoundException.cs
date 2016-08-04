using System;
using System.Runtime.Serialization;

namespace ShellRepo.Exceptions
{
    [Serializable]
    public class ShellNotFoundException : Exception
    {
        public ShellNotFoundException()
        {
        }

        public ShellNotFoundException(string message) : base(message)
        {
        }

        public ShellNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ShellNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}