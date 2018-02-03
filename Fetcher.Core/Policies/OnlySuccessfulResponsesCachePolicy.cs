using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;

namespace artm.Fetcher.Core.Policies
{
    public class OnlySuccessfulResponsesCachePolicy : IFetcherCachePolicy
    {
        public bool ShouldInsertCache(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            if (response == null) return false;
            return response.IsSuccess;
        }

        public bool ShouldUpdateCache(IFetcherWebRequest request, IUrlCacheInfo cacheHit, IFetcherWebResponse response)
        {
            if (response == null) return false;
            return response.IsSuccess;
        }
    }
}
