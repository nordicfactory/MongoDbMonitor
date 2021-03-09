using System;

namespace MongoDbCollectionMonitor.Commands.Common.Exceptions
{
    internal class NotifyStudioFailedException : Exception
    {
        public NotifyStudioFailedException(Exception innerException) :
            base("Notifying Studio failed", innerException)
        {
        }
    }
}
