using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;

namespace artm.Fetcher.Core.Policies
{
    public interface IFetcherCachePolicy
    {
        bool ShouldUpdateCache(IFetcherWebRequest request, IUrlCacheInfo cacheHit, IFetcherWebResponse response);
        bool ShouldInsertCache(IFetcherWebRequest request, IFetcherWebResponse response);
    }
}
