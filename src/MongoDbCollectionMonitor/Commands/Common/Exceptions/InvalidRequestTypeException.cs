using System;

namespace MongoDbCollectionMonitor.Commands.Common.Exceptions
{
    internal class InvalidRequestTypeException : Exception
    {
        public InvalidRequestTypeException(string assemblyName, string fullQualifiedName, Exception innerException)
            :base ($"Assembly: {assemblyName}, Full name: {fullQualifiedName} can't be resolved.", innerException)
        {
        }
    }
}
