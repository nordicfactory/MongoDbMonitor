using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace MongoDbTrigger.Bindings
{
    internal class MongoDbTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        private const string TRIGGER_DESCRIPTION = "New document change in {0}/{1} at {2}";

        internal string DatabaseName { get; set; }
        internal string Collections { get; set; }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return string.Format(TRIGGER_DESCRIPTION, DatabaseName, Collections, DateTime.UtcNow.ToString("o"));
        }
    }
}
