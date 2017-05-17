﻿using artm.Fetcher.Core.Entities;
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
        private RetryPolicy _retryPolicy = Policy
                .Handle<SQLiteException>()
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        protected IFetcherLoggerService Logger { get; set; }

        public FetcherRepositoryService(IFetcherLoggerService loggerService, Func<SQLiteConnectionWithLock> mylock) : base(mylock, null, TaskCreationOptions.None)
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

            try
            {
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
                    else
                    {
                        data = dbCache;
                    }
                }
            }
            catch(Exception ex)
            {
                Log("Could not get entry: " + ex);
                var debug = 42;
            }

            if (data != null)
            {
                data.LastAccessed = DateTimeOffset.UtcNow;
                await _retryPolicy.ExecuteAsync(() => DatabaseUpdate(data));
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

            var existingUrlCacheInfo = await GetEntryForRequestAsync(request);
            try
            {
                hero = await _retryPolicy.ExecuteAsync(() => DatabaseInsertUrlAsync(request, response, timestamp, hero, existingUrlCacheInfo));
            }
            catch (Exception ex)
            {
                Log("Could not insert entry: " + ex);
                var debug = 42;
            }
            finally
            {
                //_lock.Release();
            }

            return hero;
        }

        private async Task<UrlCacheInfo> DatabaseInsertUrlAsync(IFetcherWebRequest request, IFetcherWebResponse response, DateTimeOffset timestamp, UrlCacheInfo hero, IUrlCacheInfo existingUrlCacheInfo)
        {
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

        private async Task DatabaseUpdate(object hero)
        {
            await this.UpdateWithChildrenAsync(hero);
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
