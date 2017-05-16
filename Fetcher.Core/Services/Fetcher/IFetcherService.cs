using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherService
    {
        Task<IUrlCacheInfo> FetchAsync(Uri url);
        Task<IUrlCacheInfo> FetchAsync(Uri url, TimeSpan freshnessTreshold);
        Task<IUrlCacheInfo> FetchAsync(IFetcherWebRequest request, TimeSpan freshnessTreshold);
        Task PreloadAsync(Uri url, IFetcherWebResponse response);
    }
}
