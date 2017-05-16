using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : SQLiteAsyncConnection, IFetcherRepositoryService
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public FetcherRepositoryService(Func<SQLiteConnectionWithLock> mylock) : base(mylock, null, TaskCreationOptions.None)
        {
        }

        public async Task Initialize()
        {
            await CreateTableAsync<UrlCacheInfo>();
            await CreateTableAsync<FetcherWebResponse>();
            await CreateTableAsync<FetcherWebRequest>();
        }

        //public async Task<IUrlCacheInfo> GetEntryForUrlAsync(string url)
        //{
        //    return await GetEntryForUrlAsync(new Uri(url));
        //}

        public async Task<IUrlCacheInfo> GetEntryForRequestAsync(IFetcherWebRequest request)
        {
            IUrlCacheInfo data = null;
            if (request == null) return data;

            await _lock.WaitAsync();
            try
            {
                var dbData = await this.Table<FetcherWebRequest>()
                    .Where(x => x.Url == request.Url &&
                        x.Method == request.Method)
                    .ToListAsync();
                var dbFirst = dbData.FirstOrDefault();

                var cacheInfos = await this.Table<UrlCacheInfo>().Where(x => x.FetcherWebRequestId == request.Id).ToListAsync();
                var cacheInfo = cacheInfos.FirstOrDefault();

                if (cacheInfo != null)
                {
                    data = await this.GetWithChildrenAsync<UrlCacheInfo>(cacheInfo.Id);
                }
                else
                {
                    data = cacheInfo;
                }
            }
            catch(Exception ex)
            {
                var debug = 42;
            }
            finally
            {
                _lock.Release();
            }

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await DatabaseUpdate(data);
            }

            return data;
        }

        public async Task<IUrlCacheInfo> PreloadUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            var timestamp = DateTimeOffset.UtcNow.AddYears(-1);
            return await InsertUrlAsync(request, response, timestamp);
        }

        public async Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response)
        {
            return await InsertUrlAsync(request, response, DateTimeOffset.UtcNow);
        }

        private async Task<IUrlCacheInfo> InsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response, DateTimeOffset timestamp)
        {
            UrlCacheInfo hero = null;
            if (response == null || response as FetcherWebResponse == null)
            {
                return hero;
            }

            var existingUrlCacheInfo = await GetEntryForRequestAsync(request) as UrlCacheInfo;
            var existingRequests = await this.Table<FetcherWebRequest>().Where(x => x.Url.Equals(request.Url)).ToListAsync();
            var existingRequest = existingRequests.FirstOrDefault();
            await _lock.WaitAsync();
            try
            {
                await this.RunInTransactionAsync(tran =>
                {
                    if (existingUrlCacheInfo != null)
                    {
                        tran.Delete<FetcherWebRequest>(existingUrlCacheInfo.FetcherWebRequestId);
                        tran.Delete<IFetcherWebResponse>(existingUrlCacheInfo.FetcherWebResponseId);
                        tran.Delete(existingUrlCacheInfo);
                    }
                    if(existingRequest != null)
                    {
                        tran.Delete(existingRequest);
                    }
                    
                    tran.InsertWithChildren(request, false);
                    var theResponse = new FetcherWebResponse()
                    {
                        Body = response.Body,
                        Error = response.Error,
                        Headers = response.Headers,
                        HttpStatusCode = response.HttpStatusCode,
                    };
                    tran.InsertWithChildren(theResponse, false);

                    hero = new UrlCacheInfo()
                    {
                        FetcherWebResponseId = theResponse.Id,
                        FetcherWebResponse = theResponse,
                        FetcherWebRequestId = request.Id,
                        FetcherWebRequest = (FetcherWebRequest)request,
                        Created = timestamp,
                        LastUpdated = timestamp,
                        LastAccessed = timestamp
                    };
                    tran.InsertWithChildren(hero, true);
                });
            }
            catch(Exception ex)
            {
                var debug = 42;
            }
            finally
            {
                _lock.Release();
            }

            return hero;
        }

        private async Task DatabaseInsert(object hero)
        {
            await _lock.WaitAsync();
            try
            {
                await this.InsertWithChildrenAsync(hero, true);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task DatabaseUpdate(object hero)
        {
            await _lock.WaitAsync();
            try
            {
                await this.UpdateWithChildrenAsync(hero);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateUrlAsync(IFetcherWebRequest request, IUrlCacheInfo hero, IFetcherWebResponse response)
        {
            if (request == null || hero == null || response == null || response as FetcherWebResponse == null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            if(response.Id != 0)
            {
                hero.FetcherWebResponseId = response.Id;
                hero.FetcherWebResponse = response as FetcherWebResponse;
            }
            else
            {
                hero.FetcherWebResponse.Body = response.Body;
                hero.FetcherWebResponse.Error = response.Error;
                hero.FetcherWebResponse.Headers = response.Headers;
                hero.FetcherWebResponse.HttpStatusCode = response.HttpStatusCode;
            }

            if(request.Id != 0)
            {
                hero.FetcherWebRequestId = request.Id;
                hero.FetcherWebRequest = request as FetcherWebRequest;
            }
            else
            {
                hero.FetcherWebRequest.Headers = request.Headers;
                hero.FetcherWebRequest.Body = request.Body;
                hero.FetcherWebRequest.ContentType = request.ContentType;
                hero.FetcherWebRequest.Method = request.Method;
                
            }

            hero.FetcherWebRequest.Url = request.Url;
            hero.LastUpdated = timestamp;

            await DatabaseUpdate(hero);
        }
    }
}
