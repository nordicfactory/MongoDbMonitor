using System;

namespace MongoDbCollectionMonitor.Commands.Common.Exceptions
{
    internal class MissingRequiredPropertyException : Exception
    {
        public MissingRequiredPropertyException(string type, string propertyName)
            : base($"Type: {type} is missing required property: {propertyName}")
        {

        }
    }
}
