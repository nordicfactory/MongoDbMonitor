using System.Collections.ObjectModel;

namespace MongoDbCollectionMonitor.CrossCutting.QoS
{
    public class RetryProviderOptions
    {
        public int JitterMaximum { get; set; }

        public Collection<int> Delays { get; set; } = new Collection<int>();
    }
}
