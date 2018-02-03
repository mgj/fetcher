using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Policies
{
    public interface IFetcherCachePolicy
    {
        bool ShouldUpdateCache(IFetcherWebRequest request, IUrlCacheInfo cacheHit, IFetcherWebResponse response);
        bool ShouldInsertCache(IFetcherWebRequest request, IFetcherWebResponse response);
    }
}
