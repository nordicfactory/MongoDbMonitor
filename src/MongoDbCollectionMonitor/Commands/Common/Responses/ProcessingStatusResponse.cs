using System.Collections.Generic;

namespace MongoDbCollectionMonitor.Commands.Common.Responses
{
    public class ProcessingStatusResponse
    {
        public ProcessingStep FinalStep { get; set; }

        public bool IsSuccessfull => FinalStep == ProcessingStep.NotifyStudio;

        public IDictionary<string, int> Perf { get; set; } = new Dictionary<string, int>();
    }
}
