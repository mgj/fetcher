using artm.Fetcher.Core.Entities;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryService
    {
        Task<IUrlCacheInfo> GetEntryForUrlAsync(Uri url);

        Task UpdateUrlAsync(Uri uri, IUrlCacheInfo hero, IFetcherWebResponse response);

        Task<IUrlCacheInfo> InsertUrlAsync(Uri uri, IFetcherWebResponse response);

        Task<IUrlCacheInfo> PreloadUrlAsync(Uri uri, IFetcherWebResponse response);
    }
}
