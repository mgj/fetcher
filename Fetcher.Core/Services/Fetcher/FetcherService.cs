using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherService : IFetcherService
    {
        public readonly TimeSpan CACHE_FRESHNESS_THRESHOLD = TimeSpan.FromDays(1);

        protected IFetcherWebService WebService { get; set; }
        protected IFetcherRepositoryService Repository { get; set; }

        public FetcherService(IFetcherWebService webService, IFetcherRepositoryService repositoryService)
        {
            WebService = webService;
            Repository = repositoryService;
        }

        public async Task<IUrlCacheInfo> Fetch(Uri url)
        {
            return await Fetch(url, CACHE_FRESHNESS_THRESHOLD);
        }

        public async Task<IUrlCacheInfo> Fetch(Uri url, TimeSpan freshnessTreshold)
        {
            return await Fetch(
                new FetcherWebRequest()
                {
                    Url = url 
}, 
                freshnessTreshold);
        }

        private async Task<IUrlCacheInfo> Fetch(FetcherWebRequest request, TimeSpan freshnessTreshold)
        {
            System.Diagnostics.Debug.WriteLine("Fetching for uri: " + request.Url.OriginalString);

            var cacheHit = Repository.GetEntryForUrl(request.Url);
            if (cacheHit != null)
            {
                cacheHit.FetchedFrom = CacheSourceType.Preload;
                System.Diagnostics.Debug.WriteLine("Cache hit");
                if (ShouldInvalidate(cacheHit, freshnessTreshold))
                {
                    System.Diagnostics.Debug.WriteLine("Refreshing cache");
                    FetcherWebResponse response = null;
                    try
                    {
                        response = await FetchFromWeb(request.Url);
                        Repository.UpdateUrl(request.Url, cacheHit, response.Body);
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
                FetcherWebResponse response = null;
                try
                {
                    response = await FetchFromWeb(request.Url);
                    cacheHit = Repository.InsertUrl(request.Url, response.Body);
                    cacheHit.FetchedFrom = CacheSourceType.Web;
                    return cacheHit;
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("Could not insert from network - giving up");
                    return null;
                }
            }
        }

        private async Task<FetcherWebResponse> FetchFromWeb(Uri uri)
        {
            var policy = Policy
                .HandleResult<FetcherWebResponse>(r => r.IsSuccess == false)
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var response = await policy.ExecuteAsync(() => DoWebRequest(uri));

            return response;
        }

        public static bool ShouldInvalidate(IUrlCacheInfo hero, TimeSpan freshnessTreshold)
        {
            var delta = DateTimeOffset.UtcNow - hero.LastUpdated;
            return delta > freshnessTreshold;
        }

        private async Task<FetcherWebResponse> DoWebRequest(Uri uri)
        {
            return await Task.FromResult(WebService.DoPlatformRequest(
                new FetcherWebRequest() { Url = uri }));
        }

        public void Preload(Uri url, string response)
        {
            // Ignore if already exists in db
            var exists = Repository.GetEntryForUrl(url);
            if (exists != null)
            {
                return;
            }

            Repository.PreloadUrl(url, response);
        }
    }
}
