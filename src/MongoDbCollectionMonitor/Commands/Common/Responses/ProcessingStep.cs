using System;

namespace MongoDbCollectionMonitor.Commands.Common.Responses
{
    public enum ProcessingStep
    {
        Unknown = 0,
        ProcessMongoEvent = 1,
        ResolveCollectionType = 2,
        ExtractDocumentIdentifier = 3,
        NotifyStudio = 4,
        SendSlackAlert = 5
    }
}
