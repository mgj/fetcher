using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;

namespace artm.Fetcher.Core.Policies
{
    public class AlwaysCachePolicy : IFetcherCachePolicy
    {
        public bool ShouldInsertCache(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            return true;
        }

        public bool ShouldUpdateCache(IFetcherWebRequest request, IUrlCacheInfo cacheHit, IFetcherWebResponse response)
        {
            return true;
        }
    }
}
