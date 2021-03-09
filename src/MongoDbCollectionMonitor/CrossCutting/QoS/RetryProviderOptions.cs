using System.Collections.ObjectModel;

namespace MongoDbCollectionMonitor.CrossCutting.QoS
{
    internal class RetryProviderOptions
    {
        public int JitterMaximum { get; set; }

        public Collection<int> Delays { get; set; } = new Collection<int>();
    }
}
