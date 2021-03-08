using System;
using Microsoft.Azure.WebJobs.Description;

namespace MongoDbTrigger.Triggers
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MongoDbTriggerAttribute : Attribute
    {
    }
}
