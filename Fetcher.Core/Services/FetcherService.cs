using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using artm.Fetcher.Core.Policies;
using Newtonsoft.Json;
using Polly;
using System;
using System.Linq;
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
        protected IFetcherLoggerService Logger { get; set; }
        protected IFetcherCachePolicy CachePolicy { get; set; }

        public FetcherService(IFetcherWebService webService, IFetcherRepositoryService repositoryService, IFetcherLoggerService loggerService)
            : this(webService, repositoryService, loggerService, new OnlySuccessfulResponsesCachePolicy())
        {            
        }

        public FetcherService(IFetcherWebService webService, IFetcherRepositoryService repositoryService, IFetcherLoggerService loggerService, IFetcherCachePolicy cachePolicy)
        {
            WebService = webService;
            Repository = repositoryService;
            Logger = loggerService;
            CachePolicy = cachePolicy;
        }

        private void Log(string message)
        {
            Logger.Log("FETCHERSERVICE: " + message);
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

            Logger.Log("Fetching for uri: " + request.Url);

            await _lock.WaitAsync();
            try
            {
                var cacheHits = await Repository.GetUrlCacheInfoForRequest(request);
                if(cacheHits?.Count() > 1)
                {
                    Logger.Log($"Warning: Found {cacheHits.Count()} cache entries for request - expected maximum 1");
                }

                var cacheHit = cacheHits.FirstOrDefault();
                if (cacheHit != null)
                {
                    cacheHit.FetchedFrom = CacheSourceType.Preload;
                    Logger.Log("Cache hit");
                    if (ShouldInvalidate(cacheHit, freshnessTreshold))
                    {
                        Logger.Log("Refreshing cache");
                        try
                        {
                            IFetcherWebResponse response = await FetchFromWebAsync(request);
                            if (CachePolicy.ShouldUpdateCache(request, cacheHit, response) == true)
                            {
                                Logger.Log("CachePolicy: Updating cache entry");
                                await Repository.UpdateUrlAsync(request, cacheHit, response);
                            }
                            else
                            {
                                Logger.Log("CachePolicy: NOT caching response");
                            }
                            cacheHit.FetchedFrom = CacheSourceType.Web;
                        }
                        catch (Exception)
                        {
                            Logger.Log("Could not update from network, keep using old cache data");
                        }
                    }
                    else
                    {
                        if (DateTimeOffset.UtcNow - cacheHit.LastUpdated < TimeSpan.FromDays(365 * 9))
                        {
                            cacheHit.FetchedFrom = CacheSourceType.Local;
                        }
                    }

                    return cacheHit;
                }
                else
                {
                    Logger.Log("Nothing found in cache, getting it fresh");
                    IFetcherWebResponse response = null;
                    try
                    {
                        response = await FetchFromWebAsync(request);
                        if (CachePolicy.ShouldInsertCache(request, response) == true)
                        {
                            Logger.Log("CachePolicy: Inserting cache entry");
                            cacheHit = await Repository.InsertUrlAsync(request, response);
                            cacheHit.FetchedFrom = CacheSourceType.Web;
                        }
                        else
                        {
                            Logger.Log("CachePolicy: NOT caching response");
                        }
                        
                        return cacheHit;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Could not insert from network - giving up: " + ex);
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
                .Or<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var response = await policy.ExecuteAsync(() => DoWebRequestAsync(request));

            return response;
        }

        private async Task<IFetcherWebResponse> DoWebRequestAsync(IFetcherWebRequest request)
        {
            try
            {
                var response = await Task.FromResult(WebService.DoPlatformRequest(request));
                return response;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static bool ShouldInvalidate(IUrlCacheInfo hero, TimeSpan freshnessTreshold)
        {
            TimeSpan delta = DateTimeOffset.UtcNow - hero.LastUpdated;
            return delta > freshnessTreshold;
        }

        public async Task PreloadAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            await _lock.WaitAsync();
            try
            {
                // Ignore if already exists in db
                var exists = (await Repository.GetUrlCacheInfoForRequest(request)).FirstOrDefault();
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
