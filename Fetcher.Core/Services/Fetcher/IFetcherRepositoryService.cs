using artm.Fetcher.Core.Entities;
using System;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryService
    {
        IUrlCacheInfo GetEntryForUrl(Uri url);

        void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response);

        IUrlCacheInfo InsertUrl(Uri uri, string response);

        IUrlCacheInfo PreloadUrl(Uri uri, string response);
    }
}
