using System.Collections.ObjectModel;

namespace MongoDbCollectionMonitor
{
    public class CollectionOptions
    {
        public string Name { get; set; }

        public string AssemblyName { get; set; }

        public string HandlerRequestFullQualifiedName { get; set; }

        public Collection<string> OperationTypes { get; set; }
    }
}
