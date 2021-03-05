using System;

namespace MongoDbCollectionMonitor.Commands.Common.Exceptions
{
    internal sealed class InvalidObjectIdException : Exception
    {
        public InvalidObjectIdException(string value) : base($"Value: {value} is not a valid MongoDb identifier")
        {
        }
    }
}
