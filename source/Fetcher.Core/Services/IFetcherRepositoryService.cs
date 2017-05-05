using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryService
    {
        IUrlCacheInfo GetEntryForUrl(Uri url);

        void UpdateLastAccessed(IUrlCacheInfo hero);

        void UpdateUrl(Uri uri, IUrlCacheInfo hero, string response);

        IUrlCacheInfo InsertUrl(Uri uri, string response);
    }
}
