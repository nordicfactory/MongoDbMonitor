using System.Collections.ObjectModel;

namespace MongoDbTrigger
{
    internal class MongoDbTriggerOptions
    {
        public string ConnectionString { get; set; }

        public string Database { get; set; }

        public Collection<string> Collections { get; set; } = new Collection<string>();
    }
}
