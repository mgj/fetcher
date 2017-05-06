using artm.Fetcher.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherService
    {
        Task<IUrlCacheInfo> Fetch(Uri url);
        Task<IUrlCacheInfo> Fetch(Uri url, TimeSpan freshnessTreshold);
        void Preload(Uri url, string response);
    }
}
