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

        Task<IUrlCacheInfo> GetUrlCacheInfoForId(int id);
        
        Task UpdateUrlAsync(IFetcherWebRequest request, IUrlCacheInfo hero, IFetcherWebResponse response);

        Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response);

        Task<IUrlCacheInfo> PreloadUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response);

        Task<bool> DeleteEntry(IUrlCacheInfo hero);

        Task<bool> DeleteEntriesOlderThan(int days);

        Task<IEnumerable<IUrlCacheInfo>> GetAllUrlCacheInfo();

        Task<IEnumerable<IFetcherWebResponse>> GetAllWebResponses();

        Task<IEnumerable<IFetcherWebRequest>> GetAllWebRequests();
        Task<IEnumerable<IUrlCacheInfo>> GetUrlCacheInfoForRequest(IFetcherWebRequest needle, bool url = true, bool method = true, bool headers = true, bool contentType = true, bool body = true);
    }
}
