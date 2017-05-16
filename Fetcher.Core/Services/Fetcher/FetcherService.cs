using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherService : IFetcherService
    {
        private readonly TimeSpan DEFAULT_FRESHNESS_THRESHOLD = TimeSpan.FromDays(1);
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        protected IFetcherWebService WebService { get; set; }
        protected IFetcherRepositoryService Repository { get; set; }

        public FetcherService(IFetcherWebService webService, IFetcherRepositoryService repositoryService)
        {
            WebService = webService;
            Repository = repositoryService;
        }

        public async Task<IUrlCacheInfo> FetchAsync(Uri url)
        {
            return await FetchAsync(url, DEFAULT_FRESHNESS_THRESHOLD);
        }

        public async Task<IUrlCacheInfo> FetchAsync(Uri url, TimeSpan freshnessTreshold)
        {
            if (url == null) throw new ArgumentNullException("url");

            return await FetchAsync(
                new FetcherWebRequest()
                {
                    Url = url.OriginalString,
                    Method = "GET"
                },
                freshnessTreshold);
        }

        public async Task<IUrlCacheInfo> FetchAsync(IFetcherWebRequest request)
        {
            return await FetchAsync(request, DEFAULT_FRESHNESS_THRESHOLD);
        }

        public async Task<IUrlCacheInfo> FetchAsync(IFetcherWebRequest request, TimeSpan freshnessTreshold)
        {
            if (request == null) throw new ArgumentNullException("request");

            System.Diagnostics.Debug.WriteLine("Fetching for uri: " + request.Url);

            await _lock.WaitAsync();
            try
            {
                var cacheHit = await Repository.GetEntryForRequestAsync(request);
                if (cacheHit != null)
                {
                    cacheHit.FetchedFrom = CacheSourceType.Preload;
                    System.Diagnostics.Debug.WriteLine("Cache hit");
                    if (ShouldInvalidate(cacheHit, freshnessTreshold))
                    {
                        System.Diagnostics.Debug.WriteLine("Refreshing cache");
                        IFetcherWebResponse response = null;
                        try
                        {
                            response = await FetchFromWebAsync(request);
                            await Repository.UpdateUrlAsync(request, cacheHit, response);
                            cacheHit.FetchedFrom = CacheSourceType.Web;
                        }
                        catch (Exception)
                        {
                            System.Diagnostics.Debug.WriteLine("Could not update from network, keep using old cache data");
                        }
                    }
                    else
                    {
                        cacheHit.FetchedFrom = CacheSourceType.Local;
                    }

                    return cacheHit;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Nothing found in cache, getting it fresh");
                    IFetcherWebResponse response = null;
                    try
                    {
                        response = await FetchFromWebAsync(request);
                        cacheHit = await Repository.InsertUrlAsync(request, response);
                        cacheHit.FetchedFrom = CacheSourceType.Web;
                        return cacheHit;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Could not insert from network - giving up");
                        return null;
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<IFetcherWebResponse> FetchFromWebAsync(IFetcherWebRequest request)
        {
            var policy = Policy
                .HandleResult<IFetcherWebResponse>(r => r.IsSuccess == false)
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var response = await policy.ExecuteAsync(() => DoWebRequestAsync(request));

            return response;
        }

        public static bool ShouldInvalidate(IUrlCacheInfo hero, TimeSpan freshnessTreshold)
        {
            var delta = DateTimeOffset.UtcNow - hero.LastUpdated;
            return delta > freshnessTreshold;
        }

        private async Task<IFetcherWebResponse> DoWebRequestAsync(IFetcherWebRequest request)
        {
            return await Task.FromResult(WebService.DoPlatformRequest(request));
        }

        public async Task PreloadAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            await _lock.WaitAsync();
            try
            {
                // Ignore if already exists in db
                var exists = await Repository.GetEntryForRequestAsync(request);
                if (exists != null)
                {
                    return;
                }

                await Repository.PreloadUrlAsync(request, response);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
