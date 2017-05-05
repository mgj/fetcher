using Fetcher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fetcher.Core.Services
{
    public class FetcherService : IFetcherService
    {
        public readonly TimeSpan CACHE_FRESHNESS_THRESHOLD = TimeSpan.FromDays(1); // 1 day

        private readonly IFetcherRepositoryService _repository;
        private readonly IFetcherWebService _webService;

        public FetcherService(IFetcherRepositoryService repositoryService, IFetcherWebService webService)
        {
            _repository = repositoryService;
            _webService = webService;
        }

        public async Task<IUrlCacheInfo> Fetch(Uri url)
        {
            return await Fetch(url, CACHE_FRESHNESS_THRESHOLD);
        }

        public async Task<IUrlCacheInfo> Fetch(Uri uri, TimeSpan freshnessTreshold)
        {
            System.Diagnostics.Debug.WriteLine("Fetching for uri: " + uri.OriginalString);

            var cacheHit = _repository.GetEntryForUrl(uri);
            if (cacheHit != null)
            {
                // Hit
                System.Diagnostics.Debug.WriteLine("Cache hit");
                _repository.UpdateLastAccessed(cacheHit);
                if (ShouldInvalidate(cacheHit, freshnessTreshold))
                {
                    // Cache needs refreshing
                    System.Diagnostics.Debug.WriteLine("Refreshing cache");
                    var response = await FetchFromWeb(uri);
                    _repository.UpdateUrl(uri, cacheHit, response);
                }

                return cacheHit;
            }
            else
            {
                // Nothing in cache, get it fresh
                var response = await FetchFromWeb(uri);
                return _repository.InsertUrl(uri, response);
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

        private bool ShouldInvalidate(IUrlCacheInfo hero, TimeSpan freshnessTreshold)
        {
            var delta = DateTimeOffset.UtcNow - hero.LastUpdated;
            return delta > freshnessTreshold;
        }

        private async Task<FetcherWebResponse> DoWebRequest(Uri uri)
        {
            return await Task.FromResult(_webService.DoPlatformWebRequest(uri));
        }

        public void Preload(Uri url, string response)
        {
            // Ignore if already exists in db
            var exists = _repository.GetEntryForUrl(url);
            if (exists != null)
            {
                return;
            }

            _repository.InsertUrl(url, response);
        }
    }
}
