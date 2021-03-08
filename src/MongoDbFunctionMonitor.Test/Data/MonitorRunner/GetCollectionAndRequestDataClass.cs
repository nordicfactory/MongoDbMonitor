using System.Collections;
using System.Collections.Generic;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccount;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo;
using BannerflowDbCollectionMonitor.Commands.ProcessBrand;
using BannerflowDbCollectionMonitor.Commands.ProcessFeed;
using BannerflowDbCollectionMonitor.Commands.ProcessFolder;
using BannerflowDbCollectionMonitor.Commands.ProcessLocalization;
using BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat;

namespace MongoDbMonitor.Test.Data.MonitorRunner
{
    internal class GetCollectionAndRequestDataClass : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "DataAccount", new ProcessAccountEventRequest() };
            yield return new object[] { "DataAccountInfo", new ProcessAccountInfoEventRequest()};
            yield return new object[] { "BF_Brand", new ProcessBrandEventRequest()};
            yield return new object[] { "BF_Localization", new  ProcessLocalizationEventRequest()};
            yield return new object[] { "BF_SizeFormat", new ProcessSizeFormatEventRequest()};
            yield return new object[] { "BF_Folder", new ProcessFolderEventRequest()};
            yield return new object[] { "BF_Feed", new ProcessFeedEventRequest()};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
