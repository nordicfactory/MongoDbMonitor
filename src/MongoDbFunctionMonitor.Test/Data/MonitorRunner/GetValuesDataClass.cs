using System.Collections;
using System.Collections.Generic;

namespace MongoDbMonitor.Test.Data.MonitorRunner
{
    internal class GetValuesDataClass : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { null };
            yield return new object[] { new Dictionary<string, object>(0) };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
