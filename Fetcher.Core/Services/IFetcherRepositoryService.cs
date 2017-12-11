using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public interface IFetcherRepositoryService
    {
        Task<IUrlCacheInfo> GetEntryForRequestAsync(IFetcherWebRequest request);
        
        Task UpdateUrlAsync(IFetcherWebRequest request, IUrlCacheInfo hero, IFetcherWebResponse response);

        Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response);

        Task<IUrlCacheInfo> PreloadUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response);

        Task<bool> DeleteEntry(IUrlCacheInfo hero);

        Task<IEnumerable<UrlCacheInfo>> GetAllUrlCacheInfo();

        Task<IEnumerable<FetcherWebResponse>> GetAllWebResponses();

        Task<IEnumerable<FetcherWebRequest>> GetAllWebRequests();
    }
}
