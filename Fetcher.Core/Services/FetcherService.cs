using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
using System;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherService : IFetcherService
    {
        public readonly TimeSpan CACHE_FRESHNESS_THRESHOLD = TimeSpan.FromDays(1); // 1 day
        
        protected IFetcherWebService Webservice { get; set; }
        protected IFetcherRepositoryService Repository { get; set; }

        public FetcherService(IFetcherWebService webService, IFetcherRepositoryService repositoryService)
        {
            Webservice = webService;
            Repository = repositoryService;
        }

        public async Task<IUrlCacheInfo> Fetch(Uri url)
        {
            return await Fetch(url, CACHE_FRESHNESS_THRESHOLD);
        }

        public async Task<IUrlCacheInfo> Fetch(Uri uri, TimeSpan freshnessTreshold)
        {
            System.Diagnostics.Debug.WriteLine("Fetching for uri: " + uri.OriginalString);

            var cacheHit = Repository.GetEntryForUrl(uri);
            if (cacheHit != null)
            {
                cacheHit.FetchedFrom = CacheSourceType.Preload;
                System.Diagnostics.Debug.WriteLine("Cache hit");
                if (ShouldInvalidate(cacheHit, freshnessTreshold))
                {
                    System.Diagnostics.Debug.WriteLine("Refreshing cache");
                    string response = null;
                    try
                    {
                        response = await FetchFromWeb(uri);
                        Repository.UpdateUrl(uri, cacheHit, response);
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
                string response = null;
                try
                {
                    response = await FetchFromWeb(uri);
                    cacheHit = Repository.InsertUrl(uri, response);
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

        protected virtual async Task<string> FetchFromWeb(Uri uri)
        {
            var policy = Policy
                .HandleResult<FetcherWebResponse>(r => r.IsSuccess == false)
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var response = await policy.ExecuteAsync(() => DoWebRequest(uri));

            return response.Body;
        }

        public static bool ShouldInvalidate(IUrlCacheInfo hero, TimeSpan freshnessTreshold)
        {
            var delta = DateTimeOffset.UtcNow - hero.LastUpdated;
            return delta > freshnessTreshold;
        }

        private async Task<FetcherWebResponse> DoWebRequest(Uri uri)
        {
            return await Task.FromResult(Webservice.DoPlatformWebRequest(uri));
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
