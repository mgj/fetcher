using artm.Fetcher.Core.Entities;
using artm.Fetcher.Core.Models;
using Polly;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;

namespace artm.Fetcher.Core.Services
{
    public class FetcherRepositoryService : SQLiteAsyncConnection, IFetcherRepositoryService
    {
        protected IFetcherLoggerService Logger { get; set; }

        public FetcherRepositoryService(IFetcherLoggerService loggerService, Func<SQLiteConnectionWithLock> mylock) : base(mylock)
        {
            Logger = loggerService;
        }

        private void Log(string message)
        {
            Logger.Log("FETCHERREPOSITORYSERVICE: " + message);
        }

        public async Task Initialize()
        {
            await CreateTableAsync<UrlCacheInfo>();
            await CreateTableAsync<FetcherWebResponse>();
            await CreateTableAsync<FetcherWebRequest>();
        }

        public async Task<IUrlCacheInfo> GetEntryForRequestAsync(IFetcherWebRequest request)
        {
            IUrlCacheInfo data = null;
            if (request == null) return data;

            var dbRequest = await this.Table<FetcherWebRequest>()
                    .Where(x => x.Url == request.Url &&
                        x.Method == request.Method)
                    .FirstOrDefaultAsync();

            if (dbRequest != null)
            {
                var dbCache = await this.Table<UrlCacheInfo>().Where(x => x.FetcherWebRequestId == dbRequest.Id).FirstOrDefaultAsync();
                if (dbCache != null)
                {
                    data = await this.GetWithChildrenAsync<UrlCacheInfo>(dbCache.Id);
                }
            }

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await this.UpdateWithChildrenAsync(data);
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
            IUrlCacheInfo hero = null;
            if (response == null || response as FetcherWebResponse == null)
            {
                return hero;
            }

            try
            {
                hero = await DatabaseInsertUrlAsync(request, response, timestamp);
            }
            catch (Exception ex)
            {
                Log("Could not insert entry: " + ex);
                var debug = 42;
                throw;
            }
            finally
            {
                //_lock.Release();
            }

            return hero;
        }

        private async Task<IUrlCacheInfo> DatabaseInsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response, DateTimeOffset timestamp)
        {
            var existingUrlCacheInfo = await GetEntryForRequestAsync(request);
            UrlCacheInfo hero = null;
            await this.RunInTransactionAsync(tran =>
            {
                if (existingUrlCacheInfo != null)
                {
                    tran.Delete<FetcherWebRequest>(existingUrlCacheInfo.FetcherWebRequestId);
                    tran.Delete<IFetcherWebResponse>(existingUrlCacheInfo.FetcherWebResponseId);
                    tran.Delete(existingUrlCacheInfo);
                }

                tran.InsertWithChildren(request, false);
                var theResponse = new FetcherWebResponse()
                {
                    Body = response.Body,
                    BodyAsBytes = response.BodyAsBytes,
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
            return hero;
        }

        public async Task UpdateUrlAsync(IFetcherWebRequest request, IUrlCacheInfo toBeUpdated, IFetcherWebResponse response)
        {
            if (request == null || toBeUpdated == null || response == null || response as FetcherWebResponse == null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            if(response.Id != 0)
            {
                toBeUpdated.FetcherWebResponseId = response.Id;
                toBeUpdated.FetcherWebResponse = response as FetcherWebResponse;
            }
            toBeUpdated.FetcherWebResponse.Body = response.Body;
            toBeUpdated.FetcherWebResponse.BodyAsBytes = response.BodyAsBytes;
            toBeUpdated.FetcherWebResponse.Error = response.Error;
            toBeUpdated.FetcherWebResponse.Headers = response.Headers;
            toBeUpdated.FetcherWebResponse.HttpStatusCode = response.HttpStatusCode;

            if (request.Id != 0)
            {
                toBeUpdated.FetcherWebRequestId = request.Id;
                toBeUpdated.FetcherWebRequest = request as FetcherWebRequest;
            }
            toBeUpdated.FetcherWebRequest.Headers = request.Headers;
            toBeUpdated.FetcherWebRequest.Body = request.Body;
            toBeUpdated.FetcherWebRequest.ContentType = request.ContentType;
            toBeUpdated.FetcherWebRequest.Method = request.Method;

            toBeUpdated.FetcherWebRequest.Url = request.Url;
            toBeUpdated.LastUpdated = timestamp;

            await this.UpdateWithChildrenAsync(toBeUpdated);
        }
    }
}
